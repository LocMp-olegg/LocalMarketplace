namespace LocMp.Catalog.Application.DTOs;

public sealed record ProductPhotoDto(
    Guid Id,
    Guid ProductId,
    string StorageUrl,
    string MimeType,
    int SortOrder,
    bool IsMain,
    DateTimeOffset UploadedAt
);
