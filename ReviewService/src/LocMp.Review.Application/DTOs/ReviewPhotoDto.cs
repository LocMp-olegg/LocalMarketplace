namespace LocMp.Review.Application.DTOs;

public sealed record ReviewPhotoDto(
    Guid Id,
    string StorageUrl,
    string MimeType,
    long FileSize,
    int SortOrder);
