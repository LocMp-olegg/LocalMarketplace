namespace LocMp.Catalog.Application.DTOs;

public sealed record ShopPhotoDto
{
    public Guid Id { get; init; }
    public Guid ShopId { get; init; }
    public string StorageUrl { get; init; } = null!;
    public string MimeType { get; init; } = null!;
    public int SortOrder { get; init; }
    public DateTimeOffset UploadedAt { get; init; }
}
