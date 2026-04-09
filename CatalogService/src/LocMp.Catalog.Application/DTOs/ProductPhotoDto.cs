namespace LocMp.Catalog.Application.DTOs;

public sealed record ProductPhotoDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string StorageUrl { get; init; } = null!;
    public string MimeType { get; init; } = null!;
    public int SortOrder { get; init; }
    public bool IsMain { get; init; }
    public DateTimeOffset UploadedAt { get; init; }
}
