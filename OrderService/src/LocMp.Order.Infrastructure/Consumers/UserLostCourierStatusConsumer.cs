using LocMp.Contracts.Identity;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocMp.Order.Infrastructure.Consumers;

public sealed class UserLostCourierStatusConsumer(
    OrderDbContext db,
    ILogger<UserLostCourierStatusConsumer> logger)
    : IConsumer<UserLostCourierStatusEvent>
{
    public async Task Consume(ConsumeContext<UserLostCourierStatusEvent> context)
    {
        var courierId = context.Message.UserId;
        var ct = context.CancellationToken;

        var pendingApplications = await db.CourierApplications
            .Where(a => a.CourierId == courierId && a.Status == CourierApplicationStatus.Pending)
            .ToListAsync(ct);

        if (pendingApplications.Count == 0) return;

        var now = DateTimeOffset.UtcNow;
        foreach (var app in pendingApplications)
        {
            app.Status = CourierApplicationStatus.Withdrawn;
            app.UpdatedAt = now;
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Withdrew {Count} pending courier applications for user {UserId} who lost courier status",
            pendingApplications.Count, courierId);
    }
}