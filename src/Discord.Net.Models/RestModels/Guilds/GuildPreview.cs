using System.Text.Json.Serialization;

namespace Discord.Models.Json;

public sealed class GuildPreview : IEntityModelSource
{
    [JsonPropertyName("id")]
    public ulong Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("splash")]
    public string? Splash { get; set; }

    [JsonPropertyName("discovery_splash")]
    public string? DiscoverySplash { get; set; }

    [JsonPropertyName("emojis")]
    public required Emoji[] Emojis { get; set; }

    [JsonPropertyName("features")]
    public required string[] Features { get; set; }

    [JsonPropertyName("approximate_member_count")]
    public int ApproximateMemberCount { get; set; }

    [JsonPropertyName("approximate_presence_count")]
    public int ApproximatePresenceCount { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("stickers")]
    public required Sticker[] Stickers { get; set; }

    public IEnumerable<IEntityModel> GetEntities() => [..Emojis, ..Stickers];
}