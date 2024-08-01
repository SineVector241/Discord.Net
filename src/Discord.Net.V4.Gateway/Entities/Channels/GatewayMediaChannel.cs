using Discord.Gateway.State;
using Discord.Models;
using System.Collections.Immutable;
using static Discord.Template;

namespace Discord.Gateway;

public sealed partial class GatewayMediaChannelActor(
    DiscordGatewayClient client,
    GuildIdentity guild,
    MediaChannelIdentity channel
) :
    GatewayThreadableChannelActor(client, guild, channel),
    IMediaChannelActor,
    IGatewayCachedActor<ulong, GatewayMediaChannel, MediaChannelIdentity, IGuildMediaChannelModel>
{
    [SourceOfTruth] internal override MediaChannelIdentity Identity { get; } = channel;

    [ProxyInterface]
    internal GatewayIntegrationChannelActor IntegrationChannelActor { get; } = new(client, guild, channel);

    [SourceOfTruth]
    internal GatewayMediaChannel CreateEntity(IGuildMediaChannelModel model)
        => Client.StateController.CreateLatent(this, model, CachePath);
}

[ExtendInterfaceDefaults]
public sealed partial class GatewayMediaChannel :
    GatewayThreadableChannel,
    IMediaChannel,
    ICacheableEntity<GatewayMediaChannel, ulong, IGuildMediaChannelModel>
{
    public bool IsNsfw => Model.IsNsfw;

    public string? Topic => Model.Topic;

    public ThreadArchiveDuration DefaultAutoArchiveDuration => (ThreadArchiveDuration)Model.DefaultAutoArchiveDuration;

    public IReadOnlyCollection<ForumTag> AvailableTags { get; private set; }

    public int? ThreadCreationSlowmode => Model.DefaultThreadRateLimitPerUser;

    public ILoadableEntity<IEmote> DefaultReactionEmoji => throw new NotImplementedException();

    public SortOrder? DefaultSortOrder => (SortOrder?)Model.DefaultSortOrder;

    [ProxyInterface]
    internal override GatewayMediaChannelActor Actor { get; }

    internal override IGuildMediaChannelModel Model => _model;

    private IGuildMediaChannelModel _model;

    public GatewayMediaChannel(
        DiscordGatewayClient client,
        GuildIdentity guild,
        IGuildMediaChannelModel model,
        GatewayMediaChannelActor? actor = null,
        IEntityHandle<ulong, GatewayMediaChannel>? implicitHandle = null
    ) : base(client, guild, model, actor, implicitHandle)
    {
        _model = model;
        Actor = actor ?? new(client, guild, MediaChannelIdentity.Of(this));

        AvailableTags = model.AvailableTags
            .Select(x => ForumTag.Construct(client, new(guild.Id), x))
            .ToImmutableList();
    }

    public static GatewayMediaChannel Construct(
        DiscordGatewayClient client,
        ICacheConstructionContext<ulong, GatewayMediaChannel> context,
        IGuildMediaChannelModel model
    ) => new(
        client,
        context.Path.GetIdentity(T<GuildIdentity>(), model.GuildId),
        model,
        context.TryGetActor(T<GatewayMediaChannelActor>()),
        context.ImplicitHandle
    );

    [CovariantOverride]
    public ValueTask UpdateAsync(
        IGuildMediaChannelModel model,
        bool updateCache = true,
        CancellationToken token = default)
    {
        if (updateCache) return UpdateCacheAsync(this, model, token);

        if(!Model.AvailableTags.SequenceEqual(model.AvailableTags))
            AvailableTags = model.AvailableTags
                .Select(x => ForumTag.Construct(Client, new(Guild.Id), x))
                .ToImmutableList();

        _model = model;

        return base.UpdateAsync(model, false, token);
    }

    public override IGuildMediaChannelModel GetModel() => Model;
}
