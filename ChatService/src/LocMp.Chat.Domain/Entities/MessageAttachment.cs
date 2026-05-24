using LocMp.BuildingBlocks;
using LocMp.Chat.Domain.Enums;

namespace LocMp.Chat.Domain.Entities;

public class MessageAttachment(Guid id) : Entity<Guid>(id)
{
    public Guid MessageId { get; set; }
    public string FileName { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public MediaType MediaType { get; set; }
    public long FileSize { get; set; }
    public string StorageKey { get; set; } = null!;
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual Message Message { get; set; } = null!;
}
