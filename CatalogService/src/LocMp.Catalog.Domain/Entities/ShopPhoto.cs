namespace LocMp.Catalog.Domain.Entities;

public class ShopPhoto(Guid id)
{
    public Guid Id { get; set; } = id;
    public Guid ShopId { get; set; }
    public string StorageUrl { get; set; } = null!;
    public string ObjectKey { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public long FileSize { get; set; }
    public int SortOrder { get; set; }
    public DateTimeOffset UploadedAt { get; set; }

    public virtual Shop Shop { get; set; } = null!;
}
