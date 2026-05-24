using LocMp.BuildingBlocks;
using LocMp.Chat.Domain.Enums;

namespace LocMp.Chat.Domain.Entities;

public class ChatParticipant(Guid id) : Entity<Guid>(id)
{
    public Guid ChatId { get; set; }
    public Guid UserId { get; set; }
    public ParticipantRole Role { get; set; }
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastReadAt { get; set; }

    public virtual Chat Chat { get; set; } = null!;
}
