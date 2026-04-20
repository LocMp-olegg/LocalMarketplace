using LocMp.Notification.Application.DTOs;
using MediatR;

namespace LocMp.Notification.Application.Notifications.Queries.GetPreferences;

public sealed record GetPreferencesQuery(Guid UserId) : IRequest<NotificationPreferenceDto>;
