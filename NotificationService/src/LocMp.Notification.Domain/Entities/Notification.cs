using System.Text.Json;
using LocMp.BuildingBlocks;
using LocMp.Notification.Domain.Enums;

namespace LocMp.Notification.Domain.Entities;

public class Notification(Guid id) : Entity<Guid>(id)
{
    public Guid UserId { get; set; }

    public NotificationType Type { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;

    public DeliveryChannel DeliveryChannel { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Pending;
    public DateTimeOffset? SentAt { get; set; }

    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }

    public JsonDocument? Payload { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
