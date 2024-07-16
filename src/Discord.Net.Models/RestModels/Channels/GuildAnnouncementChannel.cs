using Discord.Converters;
using System.Text.Json.Serialization;

namespace Discord.Models.Json;

[ChannelTypeOf(ChannelType.GuildAnnouncement)]
public sealed class GuildAnnouncementChannelModel : GuildTextChannelModel, IGuildNewsChannelModel;
