using LocMp.Notification.Application.DTOs;
using LocMp.Notification.Domain.Entities;
using LocMp.Notification.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Notification.Application.Notifications.Commands.UpdatePreferences;

public sealed class UpdatePreferencesCommandHandler(NotificationDbContext db, IDistributedCache cache)
    : IRequestHandler<UpdatePreferencesCommand, NotificationPreferenceDto>
{
    public async Task<NotificationPreferenceDto> Handle(UpdatePreferencesCommand request, CancellationToken ct)
    {
        var prefs = await db.UserNotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, ct);

        if (prefs is null)
        {
            prefs = new UserNotificationPreference { UserId = request.UserId };
            db.UserNotificationPreferences.Add(prefs);
        }

        prefs.OrderUpdates = request.OrderUpdates;
        prefs.ReviewReplies = request.ReviewReplies;
        prefs.SystemAlerts = request.SystemAlerts;
        prefs.EmailEnabled = request.EmailEnabled;
        prefs.EmailOrderUpdates = request.EmailOrderUpdates;
        prefs.EmailReviewReplies = request.EmailReviewReplies;
        prefs.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync($"notif:prefs:{request.UserId}", ct);

        return new NotificationPreferenceDto(
            prefs.OrderUpdates, prefs.ReviewReplies, prefs.SystemAlerts,
            prefs.EmailEnabled, prefs.EmailOrderUpdates, prefs.EmailReviewReplies);
    }
}
