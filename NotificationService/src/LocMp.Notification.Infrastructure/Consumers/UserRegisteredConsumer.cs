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
        var exists = await db.UserNotificationPreferences.AnyAsync(p => p.UserId == msg.UserId, ctx.CancellationToken);
        if (!exists)
        {
            db.UserNotificationPreferences.Add(new UserNotificationPreference { UserId = msg.UserId });
            await db.SaveChangesAsync(ctx.CancellationToken);
        }
    }
}
