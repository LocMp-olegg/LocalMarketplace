using LocMp.BuildingBlocks;

namespace LocMp.Catalog.Domain.Entities;

public class ProductPhoto(Guid id) : Entity<Guid>(id)
{
    public Guid ProductId { get; set; }

    public string StorageUrl { get; set; } = null!;
    public string ObjectKey { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public long FileSize { get; set; }
    public int SortOrder { get; set; }
    public bool IsMain { get; set; }

    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual Product Product { get; set; } = null!;
}