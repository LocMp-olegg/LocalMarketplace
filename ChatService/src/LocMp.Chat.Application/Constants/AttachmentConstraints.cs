namespace LocMp.Chat.Application.Constants;

public static class AttachmentConstraints
{
    public static readonly IReadOnlySet<string> AllowedImageMimeTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp"
        };

    public static readonly IReadOnlySet<string> AllowedVideoMimeTypes =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "video/mp4",
            "video/webm",
            "video/quicktime"
        };

    public static readonly IReadOnlySet<string> AllAllowedMimeTypes =
        new HashSet<string>(AllowedImageMimeTypes.Concat(AllowedVideoMimeTypes), StringComparer.OrdinalIgnoreCase);

    public const long MaxImageSizeBytes = 10L * 1024 * 1024; // 10 MB
    public const long MaxVideoSizeBytes = 100L * 1024 * 1024; // 100 MB

    public const int MaxAttachmentsPerMessage = 5;
    public const int MaxMessageBodyLength = 4000;
}