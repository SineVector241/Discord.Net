namespace Discord;

/// <summary>
///     Provides properties that are used to modify an <see cref="IVoiceChannel" /> with the specified changes.
/// </summary>
public class ModifyVoiceChannelProperties : ModifyTextChannelProperties
{
    /// <summary>
    ///     Gets or sets the bitrate of the voice connections in this channel. Must be greater than 8000.
    /// </summary>
    public Optional<int> Bitrate { get; set; }
    /// <summary>
    ///     Gets or sets the maximum number of users that can be present in a channel, or <see langword="null" /> if none.
    /// </summary>
    public Optional<int?> UserLimit { get; set; }
    /// <summary>
    ///     Gets or sets the channel voice region id, automatic when set to <see langword="null"/>.
    /// </summary>
    public Optional<string?> RTCRegion { get; set; }

    /// <summary>
    ///     Get or sets the video quality mode for this channel.
    /// </summary>
    public Optional<VideoQualityMode> VideoQualityMode { get; set; }

    /// <remarks>
    ///     <b>Not supported in voice channels</b>
    /// </remarks>
    /// <inheritdoc cref="ModifyTextChannelProperties.Topic"/>
    [Obsolete("Not supported in voice channels", true)]
    public new Optional<string?> Topic { get; }
}