using LocMp.BuildingBlocks;
using LocMp.Chat.Domain.Enums;

namespace LocMp.Chat.Domain.Entities;

public class Chat(Guid id) : AggregateRoot<Guid>(id)
{
    public ChatType Type { get; set; }
    public ChatStatus Status { get; set; }
    public Guid? ReferenceId { get; set; }
    public string EncryptionKey { get; set; } = null!;
    public string? InitiatorName { get; set; }
    public string? TargetName { get; set; }
    public DateTimeOffset? LastMessageAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ClosedAt { get; set; }

    public virtual ICollection<ChatParticipant> Participants { get; set; } = [];
    public virtual ICollection<Message> Messages { get; set; } = [];
}
