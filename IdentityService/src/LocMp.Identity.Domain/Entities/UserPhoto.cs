namespace LocMp.Identity.Domain.Entities;

public class UserPhoto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public string StorageUrl { get; set; } = null!;
    public string ObjectKey { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public long FileSize { get; set; }

    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual ApplicationUser User { get; set; } = null!;
}
