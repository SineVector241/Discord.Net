using System.Text.Json.Serialization;

namespace Discord.Models.Json;

public sealed class EmbedVideo : IEmbedVideoModel
{
    [JsonPropertyName("url")]
    public Optional<string> Url { get; set; }

    [JsonPropertyName("proxy_url")]
    public Optional<string> ProxyUrl { get; set; }

    [JsonPropertyName("height")]
    public Optional<int> Height { get; set; }

    [JsonPropertyName("width")]
    public Optional<int> Width { get; set; }

    string? IEmbedVideoModel.ProxyUrl => ~ProxyUrl;
    int? IEmbedVideoModel.Height => ~Height;
    int? IEmbedVideoModel.Width => ~Width;
    string? IEmbedVideoModel.Url => ~Url;
}
