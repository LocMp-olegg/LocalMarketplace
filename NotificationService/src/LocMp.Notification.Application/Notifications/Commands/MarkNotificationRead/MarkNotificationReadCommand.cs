using MediatR;

namespace LocMp.Notification.Application.Notifications.Commands.MarkNotificationRead;

public sealed record MarkNotificationReadCommand(Guid NotificationId, Guid UserId) : IRequest;
