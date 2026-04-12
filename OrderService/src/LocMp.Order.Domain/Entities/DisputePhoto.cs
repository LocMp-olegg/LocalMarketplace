using LocMp.BuildingBlocks;

namespace LocMp.Order.Domain.Entities;

public class DisputePhoto(Guid id) : Entity<Guid>(id)
{
    public Guid DisputeId { get; set; }
    public Guid UploadedById { get; set; }

    public string StorageUrl { get; set; } = null!;
    public string ObjectKey { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public long FileSize { get; set; }
    public int SortOrder { get; set; }

    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual Dispute Dispute { get; set; } = null!;
}
