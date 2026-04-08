namespace LocMp.Identity.Application.DTOs.UserProfile;

public sealed record UserPhotoDto(
    byte[] PhotoData,
    string MimeType,
    DateTime UploadedAt
);