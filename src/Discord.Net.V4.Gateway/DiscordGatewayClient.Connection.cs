using System.Diagnostics;
using Discord.Models;
using Discord.Models.Json;
using Discord.Rest;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Threading.Channels;

namespace Discord.Gateway;

public sealed partial class DiscordGatewayClient
{
    private enum HeartbeatSignal
    {
        Requested,
        ReceivedAck
    }

    private static readonly RecyclableMemoryStreamManager _streamManager = new();

    public bool IsConnected { get; private set; }

    public int? ShardId { get; private set; }
    public int? TotalShards { get; private set; }

    private IGatewayConnection? _connection;
    private Task? _eventProcessorTask;
    private CancellationTokenSource _eventProcessorCancellationTokenSource = new();

    [MemberNotNullWhen(true, nameof(_sessionId), nameof(_resumeGatewayUrl))]
    private bool CanResume => _sessionId is not null && _resumeGatewayUrl is not null;

    private string? _resumeGatewayUrl;
    private string? _sessionId;
    private int _sequence;
    private int _heartbeatInterval;

    private readonly Channel<HeartbeatSignal> _heartbeatSignal;

    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);

    private void StartEventProcessor()
    {
        _eventProcessorCancellationTokenSource.Cancel();

        if (_eventProcessorTask?.IsCompleted ?? false)
            _eventProcessorTask.Dispose();

        _eventProcessorCancellationTokenSource.Dispose();
        _eventProcessorCancellationTokenSource = new();

        _eventProcessorTask = Task.Run(EventProcessorLoopAsync);
    }

    private async ValueTask<Uri> GetGatewayUriAsync(
        bool shouldResume = true,
        CancellationToken token = default)
    {
        string url;

        if (shouldResume && CanResume)
        {
            url = _resumeGatewayUrl;
        }
        else
        {
            var getGatewayResponse = await Rest.RestApiClient.ExecuteAsync(
                Routes.GetGateway,
                DefaultRequestOptions,
                token
            );

            if (getGatewayResponse is null)
                throw new NullReferenceException("get gateway was null");

            url = getGatewayResponse.Url;

            _resumeGatewayUrl = null;
            _sessionId = null;
        }

        var uriBuilder =
            new StringBuilder($"{url}?v={Config.GatewayVersion}&encoding={Encoding.Identifier}");

        if (GatewayCompression is not null)
            uriBuilder.Append($"&encoding={GatewayCompression.Identifier}");

        return new Uri(uriBuilder.ToString());
    }

    public async Task ConnectAsync(CancellationToken token = default)
    {
        _logger.LogDebug("Connection requested, entering connection semaphore...");
        await _connectionSemaphore.WaitAsync(token);

        try
        {
            if (IsConnected)
            {
                // TODO: possibly throw instead of return
                _logger.LogDebug("Exiting connection request early, we're already connected");
                return;
            }

            await ConnectInternalAsync(token: token);
        }
        finally
        {
            _connectionSemaphore.Release();
            _logger.LogDebug("Released connection semaphore from connection request");
        }
    }

    public async ValueTask DisconnectAsync(CancellationToken token = default)
    {
        _logger.LogDebug("Disconnect requested, entering connection semaphore...");
        await _connectionSemaphore.WaitAsync(token);

        try
        {
            await StopGatewayConnectionAsync(true, token);
        }
        finally
        {
            _connectionSemaphore.Release();
            _logger.LogDebug("Released connection semaphore from disconnect request");
        }
    }

    public ValueTask ReconnectAsync(CancellationToken token = default)
        => ReconnectInternalAsync(token: token);

    private async ValueTask ReconnectInternalAsync(
        bool shouldResume = true,
        bool gracefulDisconnect = true,
        CancellationToken token = default)
    {
        _logger.LogDebug(
            "Starting reconnect, resuming: {ShouldResume}, graceful disconnect: {Graceful}",
            shouldResume,
            gracefulDisconnect
        );

        await _connectionSemaphore.WaitAsync(token);

        try
        {
            await StopGatewayConnectionAsync(gracefulDisconnect, token);
            await ConnectInternalAsync(shouldResume, token);
        }
        finally
        {
            _connectionSemaphore.Release();
            _logger.LogDebug("Released connection semaphore from reconnect request");
        }
    }

    private async Task ConnectInternalAsync(bool shouldResume = true, CancellationToken token = default)
    {
        _connection ??= Config.GatewayConnection.Get(this);

        var gatewayUri = await GetGatewayUriAsync(shouldResume, token);

        _logger.LogInformation("Connecting to {GatewayUri}...", gatewayUri);
        await _connection.ConnectAsync(gatewayUri, token);

        IsConnected = true;

        StartEventProcessor();
    }

    private async Task EventProcessorLoopAsync()
    {
        Task? heartbeatTask = null;

        try
        {
            using var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            using var initialMessageCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                _eventProcessorCancellationTokenSource.Token,
                timeoutTokenSource.Token
            );

            if (CanResume)
            {
                _logger.LogInformation("Attempting to resume the session");

                await SendMessageAsync(
                    CreateResumeMessage(_sessionId),
                    _eventProcessorCancellationTokenSource.Token
                );

                var dispatchQueue = new Queue<IGatewayMessage>();

                while (true)
                {
                    var message = await ReceiveGatewayMessageAsync(
                        dispatchQueue.Count == 0
                            ? initialMessageCancellationTokenSource.Token
                            : _eventProcessorCancellationTokenSource.Token
                    );

                    switch (message.OpCode)
                    {
                        case GatewayOpCode.Dispatch
                            when message.EventName is not DispatchEventNames.Resumed and not null:
                            dispatchQueue.Enqueue(message);
                            break;
                        case GatewayOpCode.Dispatch when message.EventName is DispatchEventNames.Resumed:
                            break;
                        default:
                            await ProcessMessageAsync(message, _eventProcessorCancellationTokenSource.Token);
                            break;
                    }

                    _eventProcessorCancellationTokenSource.Token.ThrowIfCancellationRequested();

                    if (message.EventName is DispatchEventNames.Resumed)
                        break;
                }

                _eventProcessorCancellationTokenSource.Token.ThrowIfCancellationRequested();

                _logger.LogInformation("Resume successful, dispatching {Count} missed events", dispatchQueue.Count);

                heartbeatTask = Task.Run(() => HeartbeatLoopAsync(
                    _heartbeatInterval,
                    _eventProcessorCancellationTokenSource.Token
                ), _eventProcessorCancellationTokenSource.Token);

                while (dispatchQueue.TryDequeue(out var dispatch))
                    await HandleDispatchAsync(
                        dispatch.EventName!,
                        dispatch.Payload,
                        _eventProcessorCancellationTokenSource.Token
                    );
            }
            else
            {
                var helloPayload = GatewayMessageUtils.AsGatewayPayloadData<IHelloPayloadData>(
                    await ReceiveGatewayMessageAsync(initialMessageCancellationTokenSource.Token),
                    GatewayOpCode.Hello
                );

                _heartbeatInterval = helloPayload.HeartbeatInterval;

                _logger.LogDebug(
                    "Received Hello from discord, sending heartbeats at an interval of {Interval}",
                    _heartbeatInterval
                );

                heartbeatTask = Task.Run(() => HeartbeatLoopAsync(
                    _heartbeatInterval,
                    _eventProcessorCancellationTokenSource.Token
                ), _eventProcessorCancellationTokenSource.Token);

                await SendMessageAsync(
                    CreateIdentityMessage(),
                    _eventProcessorCancellationTokenSource.Token
                );
            }

            _logger.LogInformation("Session initialized successfully! Beginning to process events");

            while (true)
            {
                _eventProcessorCancellationTokenSource.Token.ThrowIfCancellationRequested();

                _logger.LogDebug("Waiting for message...");
                var msg = await ReceiveGatewayMessageAsync(_eventProcessorCancellationTokenSource.Token);
                
                await ProcessMessageAsync(
                    msg,
                    _eventProcessorCancellationTokenSource.Token
                );
            }
        }
        catch (Exception x) when (x is not OperationCanceledException and not GatewayClosedException)
        {
            await IndicateGatewayFailureAsync(x);
            throw;
        }
        catch(Exception x)
        {
            _logger.LogError(x, "Exception in event processing loop");
            
            if (!_eventProcessorCancellationTokenSource.IsCancellationRequested)
                _eventProcessorCancellationTokenSource.Cancel();

            if (heartbeatTask is not null)
            {
                try
                {
                    await heartbeatTask;
                }
                catch (OperationCanceledException)
                {
                }
            }
        }
        finally
        {
            heartbeatTask?.Dispose();
        }
    }

    private IGatewayMessage CreateResumeMessage(string sessionId)
    {
        return new GatewayMessage()
        {
            OpCode = GatewayOpCode.Resume,
            Payload = new ResumePayloadData
            {
                SessionId = sessionId, SessionToken = Config.Token.Value, Sequence = _sequence
            }
        };
    }

    private IGatewayMessage CreateIdentityMessage()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        
        return new GatewayMessage
        {
            OpCode = GatewayOpCode.Identify,
            Payload = new IdentityPayloadData()
            {
                Token = Config.Token.Value,
                Properties = new IdentityConnectionProperties
                {
                    Browser = $"Discord.Net {version}",
                    OS = Environment.OSVersion.Platform.ToString(),
                    Device = $"Discord.Net {version}"
                },
                Intents = (int) Config.Intents,
                Compress = Config.UsePayloadCompression,
                LargeThreshold = Config.LargeThreshold
            }
        };
    }

#pragma warning disable CA2016
    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
    private async Task ProcessMessageAsync(IGatewayMessage message, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        _logger.LogDebug("Processing OpCode '{Code}'", message.OpCode);
        
        switch (message.OpCode)
        {
            case GatewayOpCode.Dispatch when message.EventName is not null:
                await HandleDispatchAsync(message.EventName, message.Payload, token);
                break;
            case GatewayOpCode.Heartbeat:
                await _heartbeatSignal.Writer.WriteAsync(HeartbeatSignal.Requested, token);
                break;
            case GatewayOpCode.HeartbeatAck:
                await _heartbeatSignal.Writer.WriteAsync(HeartbeatSignal.ReceivedAck, token);
                break;
            case GatewayOpCode.Reconnect:
                // Don't pass 'token', it will get cancelled for the reconnect.
                await ReconnectInternalAsync(gracefulDisconnect: false);
                break;
            case GatewayOpCode.InvalidSession:
                if (message.Payload is not IInvalidSessionPayloadData invalidSessionPayload)
                    throw new UnexpectedGatewayPayloadException(typeof(InvalidSessionPayloadData), message.Payload);

                // Don't pass 'token', it will get cancelled for the reconnect.
                await ReconnectInternalAsync(invalidSessionPayload.CanResume);
                break;
            default:
                _logger.LogWarning("Received unknown opcode '{OpCode}'", message.OpCode.ToString("X"));
                break;
        }
    }
#pragma warning restore CA2016

    private async Task HeartbeatLoopAsync(
        int interval,
        CancellationToken token,
        bool sendFirstHeartbeatInstantly = false)
    {
        var jitter = true;

        try
        {
            while (!token.IsCancellationRequested)
            {
                var heartbeatDelay = interval;

                if (jitter)
                {
                    heartbeatDelay = sendFirstHeartbeatInstantly
                        ? 0
                        : (int) Math.Floor(heartbeatDelay * Random.Shared.NextSingle());
                    jitter = false;
                }

                if (heartbeatDelay > 0)
                {
                    using var heartbeatWaitCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(token);
                    var heartbeatTimeoutTask = Task.Delay(heartbeatDelay, heartbeatWaitCancellationToken.Token);
                    var heartbeatSignalTask = _heartbeatSignal.Reader.ReadAsync(
                        heartbeatWaitCancellationToken.Token
                    ).AsTask();

                    // wait for either the delay or a heartbeat signal
                    // TODO: I don't like this '.AsTask' allocation
                    var triggeringTask = await Task.WhenAny(
                        heartbeatTimeoutTask,
                        heartbeatSignalTask
                    );

                    _logger.LogDebug(
                        "Heartbeat interrupt: {Source}",
                        triggeringTask == heartbeatTimeoutTask ? "Interval elapsed" : "Discord sent a heartbeat request"
                    );

                    // cancel any remaining parts
                    heartbeatWaitCancellationToken.Cancel();
                }

                var attempts = 0;

                while (true)
                {
                    if (attempts >= 3)
                    {
                        throw new HeartbeatUnacknowledgedException(attempts);
                    }

                    await SendMessageAsync(
                        new GatewayMessage {OpCode = GatewayOpCode.Heartbeat, Sequence = _sequence},
                        token);

                    using var heartbeatWaitCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(token);
                    heartbeatWaitCancellationToken.CancelAfter(3000);

                    try
                    {
                        var result = await _heartbeatSignal.Reader.ReadAsync(heartbeatWaitCancellationToken.Token);

                        if (result is HeartbeatSignal.ReceivedAck)
                            break;
                    }
                    catch (OperationCanceledException canceledException)
                        when (canceledException.CancellationToken == heartbeatWaitCancellationToken.Token)
                    {
                        attempts++;
                    }
                }
            }
        }
        catch (HeartbeatUnacknowledgedException ex)
        {
            _logger.LogError(ex, "Heartbeat failed");
            
            // don't pass the token, since we don't want to cancel the reconnect.
            // ReSharper disable once MethodSupportsCancellation
            await ReconnectInternalAsync(gracefulDisconnect: false);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await IndicateGatewayFailureAsync(exception);
            throw;
        }
    }

    private async ValueTask IndicateGatewayFailureAsync(Exception exception)
    {
        // TODO:
        // invoke a 'GatewayError' event in user-code land to indicate that something went wrong

        _logger.LogError(exception, "Gateway failure occured");

        await StopGatewayConnectionAsync(false);
    }

    private async ValueTask HandleGatewayClosureAsync(GatewayReadResult result)
    {
        _logger.LogInformation("Received gateway closure: {Status}", result);

        var shouldReconnect =
            result.CloseStatusCode is >= GatewayCloseCode.UnknownError
                and <= GatewayCloseCode.SessionTimedOut
                and not GatewayCloseCode.AuthenticationFailed;

        await _connectionSemaphore.WaitAsync();

        try
        {
            await StopGatewayConnectionAsync(false);

            if (shouldReconnect) await ConnectInternalAsync();
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }


    private async ValueTask StopGatewayConnectionAsync(bool graceful, CancellationToken token = default)
    {
        ShardId = null;
        TotalShards = null;

        _eventProcessorCancellationTokenSource.Cancel();

        if (_eventProcessorTask is not null && graceful)
        {
            try
            {
                _logger.LogDebug("Waiting for event processor to shutdown (graceful disconnect)...");
                await _eventProcessorTask;
            }
            catch (OperationCanceledException)
            {
            }

            _logger.LogDebug("Event processor has been successfully stopped.");
        }

        if(_eventProcessorTask?.IsCompleted ?? false)
            _eventProcessorTask?.Dispose();

        if (_connection is not null)
        {
            _logger.LogDebug("Sending disconnect request to the underlying gateway connection...");
            await _connection.DisconnectAsync(token);
        }

        IsConnected = false;
    }

    private async Task<IGatewayMessage> ReceiveGatewayMessageAsync(CancellationToken token = default)
    {
        if (_connection is null)
            throw new NullReferenceException("Connection was null");

        var stream = _streamManager.GetStream(nameof(ReceiveGatewayMessageAsync));

        try
        {
            _logger.LogDebug("Calling read on underlying gateway connection...");
            var result = await _connection.ReadAsync(stream, token);

            _logger.LogDebug("Read complete: {Result}", result);
            
            if (result.CloseStatusCode.HasValue)
            {
                await HandleGatewayClosureAsync(result);
                throw new GatewayClosedException(result);
            }

            stream.Position = 0;

            if (GatewayCompression is not null && result.Format is TransportFormat.Binary)
            {
                _logger.LogDebug("Running decompression...");
                
                var secondary = _streamManager.GetStream(nameof(ReceiveGatewayMessageAsync));
                await GatewayCompression.DecompressAsync(stream, secondary, token);
                await stream.DisposeAsync();
                stream = secondary;

                stream.Position = 0;
            }
            else if (result.Format != Encoding.Format)
            {
                // TODO:
                // 'Encoding' should specify if it can support binary data
                // if the encoding doesn't support binary or vice-versa, throw.
            }

            _logger.LogDebug("Running decoder...");
            
            var message =
                await Encoding.DecodeAsync<IGatewayMessage>(stream, token)
                ?? throw new NullReferenceException("Received a null gateway message");

            if (message.Sequence.HasValue)
                Interlocked.Exchange(ref _sequence, message.Sequence.Value);

            _logger.LogDebug("C<-S: {OpCode}: {EventName}", message.OpCode, message.EventName ?? "no dispatch");

            return message;
        }
        finally
        {
            await stream.DisposeAsync();
        }
    }

    private async Task SendMessageAsync(IGatewayMessage message, CancellationToken token = default)
    {
        if (_connection is null)
            throw new NullReferenceException("Connection was null");

        var stream = _streamManager.GetStream(nameof(ReceiveGatewayMessageAsync));

        try
        {
            await Encoding.EncodeAsync(stream, message, token);

            stream.Position = 0;

#if DEBUG
            var buffer = new byte[stream.Length];
            var sz = stream.Read(buffer, 0, buffer.Length);
            var payload = System.Text.Encoding.UTF8.GetString(buffer);
            
            _logger.LogDebug("Sending payload with {Size} bytes: {Payload}", sz, payload);

            stream.Position = 0;
#endif

            await _connection.SendAsync(stream, Encoding.Format, token);

            _logger.LogDebug("C->S: {OpCode}", message.OpCode);
        }
        finally
        {
            await stream.DisposeAsync();
        }
    }
}