using Discord.Models.Json;

namespace Discord;

public sealed class BacklinkFollowAnnouncementChannelProperties : 
    IEntityProperties<FollowAnnouncementChannelParams>
{
    [IdHeuristic<IAnnouncementChannel>]
    public required EntityOrId<ulong, IAnnouncementChannelActor> AnnouncementChannel { get; set; }

    internal ulong TargetChannelId { get; }
    
    internal BacklinkFollowAnnouncementChannelProperties(IPathable path)
    {
        TargetChannelId = path.Require<IIntegrationChannel>();
    }

    public BacklinkFollowAnnouncementChannelProperties(EntityOrId<ulong, IIntegrationChannel> targetChannel)
    {
        TargetChannelId = targetChannel;
    }
    
    public FollowAnnouncementChannelParams ToApiModel(FollowAnnouncementChannelParams? existing = default)
    {
        return new FollowAnnouncementChannelParams()
        {
            WebhookChannelId = TargetChannelId,
        };
    }
}