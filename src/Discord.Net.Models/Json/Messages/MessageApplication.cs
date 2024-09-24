using System.Text.Json.Serialization;

namespace Discord.Models.Json;

public sealed class MessageApplication : IMessageApplicationModel
{
    [JsonPropertyName("id")]
    public ulong Id { get; set; }

    [JsonPropertyName("cover_image")]
    public required string CoverImage { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("icon")]
    public required string Icon { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }
}
