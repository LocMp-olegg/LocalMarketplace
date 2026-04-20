using LocMp.Notification.Application.DTOs;
using LocMp.Notification.Domain.Entities;
using LocMp.Notification.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Notification.Application.Notifications.Queries.GetPreferences;

public sealed class GetPreferencesQueryHandler(NotificationDbContext db)
    : IRequestHandler<GetPreferencesQuery, NotificationPreferenceDto>
{
    public async Task<NotificationPreferenceDto> Handle(GetPreferencesQuery request, CancellationToken ct)
    {
        var prefs = await db.UserNotificationPreferences
                        .FirstOrDefaultAsync(p => p.UserId == request.UserId, ct)
                    ?? new UserNotificationPreference { UserId = request.UserId };

        return new NotificationPreferenceDto(
            prefs.OrderUpdates, prefs.ReviewReplies, prefs.SystemAlerts,
            prefs.EmailEnabled, prefs.EmailOrderUpdates, prefs.EmailReviewReplies);
    }
}
