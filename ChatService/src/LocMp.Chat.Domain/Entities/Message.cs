using LocMp.BuildingBlocks;
using LocMp.Chat.Domain.Enums;

namespace LocMp.Chat.Domain.Entities;

public class Message(Guid id) : Entity<Guid>(id)
{
    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }
    public string SenderName { get; set; } = null!;
    public MessageType Type { get; set; }
    public string EncryptedBody { get; set; } = string.Empty;
    public DateTimeOffset SentAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public virtual Chat Chat { get; set; } = null!;
    public virtual ICollection<MessageAttachment> Attachments { get; set; } = [];
}
