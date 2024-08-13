using Discord.Models.Json;

namespace Discord.Models;

public interface IExtendedGuild : IGuildCreatePayloadData, IGuildModel
{
    DateTimeOffset JoinedAt { get; }
    bool IsLarge { get; }
    int MemberCount { get; }
    IEnumerable<IPartialVoiceStateModel> VoiceStates { get; }
    IEnumerable<IMemberModel> Members { get; }
    IEnumerable<IGuildChannelModel> Channels { get; }
    IEnumerable<IThreadChannelModel> Threads { get; }
    IEnumerable<IPartialPresenceModel> Presences { get; }
    IEnumerable<IStageInstanceModel> StageInstances { get; }
    IEnumerable<IGuildScheduledEventModel> GuildScheduledEvents { get; }
}