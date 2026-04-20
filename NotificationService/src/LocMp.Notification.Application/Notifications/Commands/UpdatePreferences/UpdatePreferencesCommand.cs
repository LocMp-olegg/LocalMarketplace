using LocMp.Notification.Application.DTOs;
using MediatR;

namespace LocMp.Notification.Application.Notifications.Commands.UpdatePreferences;

public sealed record UpdatePreferencesCommand(
    Guid UserId,
    bool OrderUpdates,
    bool ReviewReplies,
    bool SystemAlerts,
    bool EmailEnabled,
    bool EmailOrderUpdates,
    bool EmailReviewReplies) : IRequest<NotificationPreferenceDto>;
