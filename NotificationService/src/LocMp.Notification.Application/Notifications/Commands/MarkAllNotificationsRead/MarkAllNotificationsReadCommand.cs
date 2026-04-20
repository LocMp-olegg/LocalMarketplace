using MediatR;

namespace LocMp.Notification.Application.Notifications.Commands.MarkAllNotificationsRead;

public sealed record MarkAllNotificationsReadCommand(Guid UserId) : IRequest;
