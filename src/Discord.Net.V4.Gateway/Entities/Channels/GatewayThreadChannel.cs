using Discord.Entities.Channels.Threads;
using Discord.Gateway.State;
using Discord.Models;
using System.Collections.Immutable;
using static Discord.Template;

namespace Discord.Gateway;

using MessageChannelTrait = GatewayMessageChannelTrait<
    GatewayThreadChannelActor,
    GatewayThreadChannel,
    ThreadChannelIdentity
>;

[ExtendInterfaceDefaults]
public sealed partial class GatewayThreadChannelActor :
    GatewayGuildChannelActor,
    IThreadChannelActor,
    IGatewayCachedActor<ulong, GatewayThreadChannel, ThreadChannelIdentity, IThreadChannelModel>
{
    public IThreadMemberActor CurrentThreadMember => throw new NotImplementedException();

    public IEnumerableIndexableActor<IThreadMemberActor, ulong, IThreadMember> ThreadMembers =>
        throw new NotImplementedException();

    [SourceOfTruth] internal override ThreadChannelIdentity Identity { get; }

    [ProxyInterface(typeof(IMessageChannelTrait))] internal MessageChannelTrait MessageChannelActor { get; }

    [TypeFactory]
    public GatewayThreadChannelActor(
        DiscordGatewayClient client,
        GuildIdentity guild,
        ThreadChannelIdentity thread
    ) : base(client, guild, thread)
    {
        Identity = thread | this;
        MessageChannelActor = new(client, this, thread);
    }

    [SourceOfTruth]
    internal GatewayThreadChannel CreateEntity(IThreadChannelModel model)
        => Client.StateController.CreateLatent(this, model, CachePath);
}

public sealed partial class GatewayThreadChannel :
    GatewayGuildChannel,
    IThreadChannel,
    ICacheableEntity<GatewayThreadChannel, ulong, IThreadChannelModel>
{
    [SourceOfTruth] public GatewayUserActor Creator { get; }

    public new ThreadType Type => (ThreadType)Model.Type;

    public bool HasJoined => Model.HasJoined;

    public bool IsArchived => Model.IsArchived;

    public ThreadArchiveDuration AutoArchiveDuration => (ThreadArchiveDuration)Model.AutoArchiveDuration;

    public DateTimeOffset ArchiveTimestamp => Model.ArchiveTimestamp;

    public bool IsLocked => Model.IsLocked;

    public int MemberCount => Model.MemberCount;

    public int MessageCount => Model.MessageCount;

    public bool? IsInvitable => Model.IsInvitable;

    public IReadOnlyCollection<ulong> AppliedTags { get; private set; }

    public DateTimeOffset CreatedAt => Model.CreatedAt ?? SnowflakeUtils.FromSnowflake(Id);

    [ProxyInterface] internal override GatewayThreadChannelActor Actor { get; }

    internal override IThreadChannelModel Model => _model;

    private IThreadChannelModel _model;

    public GatewayThreadChannel(
        DiscordGatewayClient client,
        GuildIdentity guild,
        IThreadChannelModel model,
        GatewayThreadChannelActor? actor = null
    ) : base(client, guild, model, actor)
    {
        _model = model;
        Actor = actor ?? new(client, guild, ThreadChannelIdentity.Of(this));

        Creator = client.Users[model.OwnerId];

        AppliedTags = model.AppliedTags.ToImmutableList();
    }

    public static GatewayThreadChannel Construct(
        DiscordGatewayClient client,
        IGatewayConstructionContext context,
        IThreadChannelModel model
    ) => new(
        client,
        context.Path.GetIdentity(T<GuildIdentity>(), model.GuildId),
        model,
        context.TryGetActor<GatewayThreadChannelActor>()
    );

    [CovariantOverride]
    public ValueTask UpdateAsync(IThreadChannelModel model, bool updateCache = true, CancellationToken token = default)
    {
        if (updateCache) return UpdateCacheAsync(this, model, token);

        if (!Model.AppliedTags.SequenceEqual(model.AppliedTags))
            AppliedTags = model.AppliedTags.ToImmutableList();

        _model = model;

        return base.UpdateAsync(model, false, token);
    }

    public override IThreadChannelModel GetModel() => Model;
}