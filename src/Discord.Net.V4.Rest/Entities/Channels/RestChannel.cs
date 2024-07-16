using Discord.Models;
using Discord.Models.Json;
using Discord.Rest.Guilds;

namespace Discord.Rest.Channels;

// public partial class RestLoadableChannelActor(
//     DiscordRestClient client,
//     ChannelIdentity channel
// ):
//     RestChannelActor(client, channel),
//     ILoadableChannelActor
// {
//     [ProxyInterface(typeof(ILoadableEntity<IChannel>))]
//     internal RestLoadable<ulong, RestChannel, IChannel, IChannelModel> Loadable { get; } =
//         RestLoadable<ulong, RestChannel, IChannel, IChannelModel>.FromConstructable<RestChannel>(
//             client,
//             channel,
//             Routes.GetChannel
//         );
// }

[ExtendInterfaceDefaults(typeof(IChannelActor))]
public partial class RestChannelActor(
    DiscordRestClient client,
    ChannelIdentity channel
) :
    RestActor<ulong, RestChannel, ChannelIdentity>(client, channel),
    IChannelActor
{
    [SourceOfTruth]
    internal virtual RestChannel CreateEntity(IChannelModel model)
        => RestChannel.Construct(Client, model);
}

public partial class RestChannel :
    RestEntity<ulong>,
    IChannel,
    IConstructable<RestChannel, IChannelModel, DiscordRestClient>,
    IContextConstructable<RestChannel, IChannelModel, GuildIdentity?, DiscordRestClient>
{
    public ChannelType Type => (ChannelType)Model.Type;

    internal virtual IChannelModel Model => _model;

    [ProxyInterface(
        typeof(IChannelActor),
        typeof(IEntityProvider<IChannel, IChannelModel>)
    )]
    internal virtual RestChannelActor Actor { get; }

    private IChannelModel _model;

    internal RestChannel(
        DiscordRestClient client,
        IChannelModel model,
        RestChannelActor? actor = null
    ) : base(client, model.Id)
    {
        _model = model;

        Actor = actor ?? new(client, ChannelIdentity.Of(this));
    }

    public static RestChannel Construct(DiscordRestClient client, IChannelModel model)
    {
        return model switch
        {
            DMChannelModel dmChannelModel => RestDMChannel.Construct(client, dmChannelModel),
            GroupDMChannelModel groupDMChannel => RestGroupChannel.Construct(client, groupDMChannel),
            GuildChannelModelBase guildChannelBase => Construct(client, model,
                GuildIdentity.Of(guildChannelBase.GuildId)),
            _ => new RestChannel(client, model)
        };
    }

    public static RestChannel Construct(
        DiscordRestClient client,
        IChannelModel model,
        GuildIdentity? guild)
    {
        switch (guild)
        {
            case null when model is IGuildChannelModel guildChannelModel:
                guild = GuildIdentity.Of(guildChannelModel.GuildId);
                break;
            case null:
                return Construct(client, model);
        }


        return model switch
        {
            IGuildChannelModel guildChannel => guildChannel switch
            {
                IGuildNewsChannelModel guildAnnouncementChannel => RestNewsChannel.Construct(client,
                    guildAnnouncementChannel, guild),
                IGuildCategoryChannelModel guildCategoryChannel => RestCategoryChannel.Construct(client,
                    guildCategoryChannel,
                    guild),
                IGuildDirectoryChannel guildDirectoryChannel => RestGuildChannel.Construct(client,
                    guildDirectoryChannel,
                    guild),
                IGuildForumChannelModel guildForumChannel => RestForumChannel.Construct(client, guildForumChannel,
                    guild),
                IGuildMediaChannelModel guildMediaChannel => RestMediaChannel.Construct(client, guildMediaChannel,
                    guild),
                IGuildStageChannelModel guildStageVoiceChannel => RestStageChannel.Construct(client,
                    guildStageVoiceChannel, guild),
                IGuildVoiceChannelModel guildVoiceChannel => RestVoiceChannel.Construct(client, guildVoiceChannel,
                    guild),
                IThreadChannelModel threadChannel => RestThreadChannel.Construct(client, threadChannel, new(guild)),
                IGuildTextChannelModel guildTextChannel => RestTextChannel.Construct(client, guildTextChannel, guild),
                _ => throw new ArgumentOutOfRangeException(nameof(guildChannel))
            },
            _ => Construct(client, model)
        };
    }

    public virtual ValueTask UpdateAsync(IChannelModel model, CancellationToken token = default)
    {
        _model = model;
        return ValueTask.CompletedTask;
    }

    public virtual IChannelModel GetModel() => Model;
}
