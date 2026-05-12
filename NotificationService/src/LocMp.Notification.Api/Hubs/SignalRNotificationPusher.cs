using LocMp.Notification.Domain;
using Microsoft.AspNetCore.SignalR;

namespace LocMp.Notification.Api.Hubs;

public sealed class SignalRNotificationPusher(IHubContext<NotificationHub> hub) : INotificationPusher
{
    public Task PushAsync(Guid userId, NotificationPushDto notification, CancellationToken ct = default)
        => hub.Clients.User(userId.ToString())
               .SendAsync("notification_received", notification, ct);
}
