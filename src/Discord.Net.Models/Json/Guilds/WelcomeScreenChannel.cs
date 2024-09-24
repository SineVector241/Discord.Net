using Discord.Models;
using System.Text.Json.Serialization;

namespace Discord.Models.Json;

public sealed class WelcomeScreenChannel : IWelcomeScreenChannelModel
{
    [JsonPropertyName("channel_id")]
    public ulong ChannelId { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("emoji_id")]
    public ulong? EmojiId { get; set; }

    [JsonPropertyName("emoji_name")]
    public string? EmojiName { get; set; }
}
