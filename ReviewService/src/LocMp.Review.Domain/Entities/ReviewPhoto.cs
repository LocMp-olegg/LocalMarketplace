using LocMp.BuildingBlocks;

namespace LocMp.Review.Domain.Entities;

public class ReviewPhoto(Guid id) : Entity<Guid>(id)
{
    public Guid ReviewId { get; set; }

    public string StorageUrl { get; set; } = null!;
    public string ObjectKey { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public long FileSize { get; set; }
    public int SortOrder { get; set; }

    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual Review Review { get; set; } = null!;
}