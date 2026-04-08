namespace LocMp.Identity.Domain.Entities;

public class UserPhoto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public byte[] PhotoData { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public virtual ApplicationUser User { get; set; } = null!;
}
