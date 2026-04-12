namespace LocMp.Order.Application.DTOs;

public sealed record DisputePhotoDto(
    Guid Id,
    Guid UploadedById,
    string StorageUrl,
    string MimeType,
    long FileSize,
    int SortOrder,
    DateTimeOffset UploadedAt);
