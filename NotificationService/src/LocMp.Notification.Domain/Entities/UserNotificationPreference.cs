namespace LocMp.Notification.Domain.Entities;

public class UserNotificationPreference
{
    public Guid UserId { get; set; }

    public bool OrderUpdates { get; set; } = true;
    public bool ReviewReplies { get; set; } = true;
    public bool SystemAlerts { get; set; } = true;

    public DateTimeOffset? UpdatedAt { get; set; }
}
