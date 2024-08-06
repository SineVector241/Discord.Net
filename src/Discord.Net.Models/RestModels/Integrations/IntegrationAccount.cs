using System.Text.Json.Serialization;

namespace Discord.Models.Json;

public sealed class IntegrationAccount : IIntegrationAccountModel
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }
}
