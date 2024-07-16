using Discord.Models;
using Discord.Models.Json;
using Discord.Rest.Guilds;
using System.Collections.Immutable;

namespace Discord.Rest.Channels;

public partial class RestForumChannelActor(
    DiscordRestClient client,
    GuildIdentity guild,
    ForumChannelIdentity channel
) :
    RestThreadableChannelActor(client, guild, channel),
    IForumChannelActor,
    IActor<ulong, RestForumChannel>
{
    [CovariantOverride]
    [SourceOfTruth]
    internal RestForumChannel CreateEntity(IGuildForumChannelModel model)
        => RestForumChannel.Construct(Client, model, Guild.Identity);
}

public partial class RestForumChannel :
    RestThreadableChannel,
    IForumChannel,
    IContextConstructable<RestForumChannel, IGuildForumChannelModel, GuildIdentity, DiscordRestClient>
{
    public bool IsNsfw => Model.IsNsfw;

    public string? Topic => Model.Topic;

    public ThreadArchiveDuration DefaultAutoArchiveDuration => (ThreadArchiveDuration)Model.DefaultAutoArchiveDuration;

    public IReadOnlyCollection<ForumTag> AvailableTags { get; private set; }

    public int? ThreadCreationSlowmode => Model.DefaultThreadRateLimitPerUser;

    public ILoadableEntity<IEmote> DefaultReactionEmoji => throw new NotImplementedException();

    public SortOrder? DefaultSortOrder => (SortOrder?)Model.DefaultSortOrder;

    public ForumLayout DefaultLayout => (ForumLayout)Model.DefaultForumLayout;

    internal override IGuildForumChannelModel Model => _model;

    [ProxyInterface(
        typeof(IForumChannelActor),
        typeof(IEntityProvider<IForumChannel, IGuildForumChannelModel>)
    )]
    internal override RestForumChannelActor Actor { get; }

    private IGuildForumChannelModel _model;

    internal RestForumChannel(
        DiscordRestClient client,
        GuildIdentity guild,
        IGuildForumChannelModel model,
        RestForumChannelActor? actor = null
    ) : base(client, guild, model, actor)
    {
        _model = model;
        Actor = actor ?? new(client, guild, ForumChannelIdentity.Of(this));

        AvailableTags = model.AvailableTags
            .Select(x => ForumTag.Construct(client, x, new ForumTag.Context(guild.Id)))
            .ToImmutableArray();
    }

    public static RestForumChannel Construct(
        DiscordRestClient client,
        IGuildForumChannelModel model,
        GuildIdentity guild
    ) => new(client, guild, model);

    [CovariantOverride]
    public ValueTask UpdateAsync(IGuildForumChannelModel model, CancellationToken token = default)
    {
        _model = model;

        AvailableTags = Model.AvailableTags
            .Select(x => ForumTag.Construct(Client, x, new ForumTag.Context(Guild.Id)))
            .ToImmutableArray();

        return base.UpdateAsync(model, token);
    }

    public override IGuildForumChannelModel GetModel() => Model;
}
