using LocMp.Contracts.Identity;
using LocMp.Notification.Domain.Entities;
using LocMp.Notification.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Notification.Infrastructure.Consumers;

public sealed class UserRegisteredConsumer(NotificationDbContext db) : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> ctx)
    {
        var msg = ctx.Message;
        var prefs = await db.UserNotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == msg.UserId, ctx.CancellationToken);

        if (prefs is null)
        {
            db.UserNotificationPreferences.Add(new UserNotificationPreference
            {
                UserId = msg.UserId,
                Email = msg.Email
            });
        }
        else if (prefs.Email is null)
        {
            prefs.Email = msg.Email;
        }

        await db.SaveChangesAsync(ctx.CancellationToken);
    }
}
