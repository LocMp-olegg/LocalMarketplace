namespace LocMp.Identity.Application.DTOs.UserProfile;

public sealed record UserPhotoDto(
    string StorageUrl,
    string MimeType,
    DateTimeOffset UploadedAt
);