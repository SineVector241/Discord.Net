namespace Discord.Models;

[ModelEquality]
public partial interface IAttachmentModel : IEntityModel<ulong>
{
    string Filename { get; }
    string? Description { get; }
    string? ContentType { get; }
    int Size { get; }
    string Url { get; }
    string ProxyUrl { get; }
    int? Height { get; }
    int? Width { get; }
    bool Ephemeral { get; }
    float? Duration { get; }
    string? Waveform { get; }
    int? Flags { get; }
}