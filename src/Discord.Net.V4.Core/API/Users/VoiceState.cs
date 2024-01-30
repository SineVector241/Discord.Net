using System.Text.Json.Serialization;

namespace Discord.API;

public sealed class VoiceState
{
    [JsonPropertyName("guild_id")]
    public Optional<ulong> GuildId { get; set; }

    [JsonPropertyName("channel_id")]
    public ulong? ChannelId { get; set; }

    [JsonPropertyName("user_id")]
    public ulong UserId { get; set; }

    // ALWAYS sent over WebSocket, never on REST
    [JsonPropertyName("member")]
    public Optional<GuildMember> Member { get; set; }

    [JsonPropertyName("session_id")]
    public required string SessionId { get; set; }

    [JsonPropertyName("deaf")]
    public bool Deaf { get; set; }

    [JsonPropertyName("mute")]
    public bool Mute { get; set; }

    [JsonPropertyName("self_deaf")]
    public bool SelfDeaf { get; set; }

    [JsonPropertyName("self_mute")]
    public bool SelfMute { get; set; }

    [JsonPropertyName("self_stream")]
    public bool? SelfStream { get; set; }

    [JsonPropertyName("self_video")]
    public bool SelfVideo { get; set; }

    [JsonPropertyName("suppress")]
    public bool Suppress { get; set; }

    [JsonPropertyName("request_to_speak_timestamp")]
    public DateTimeOffset? RequestToSpeakTimestamp { get; set; }
}