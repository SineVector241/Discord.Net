using System.Text.Json.Serialization;

namespace Discord.Models.Json;

public sealed class ModifyMessageParams : AttachmentUploadParams
{
    [JsonPropertyName("content")]
    public Optional<string> Content { get; set; }

    [JsonPropertyName("embeds")]
    public Optional<Embed[]> Embeds { get; set; }

    [JsonPropertyName("flags")]
    public Optional<int> Flags { get; set; }

    [JsonPropertyName("allowed_mentions")]
    public Optional<AllowedMentions> AllowedMentions { get; set; }

    [JsonPropertyName("components")]
    public Optional<MessageComponent[]> Components { get; set; }
}
