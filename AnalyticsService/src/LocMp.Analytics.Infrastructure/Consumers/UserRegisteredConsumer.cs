using LocMp.Analytics.Domain.Entities;
using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.Contracts.Identity;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocMp.Analytics.Infrastructure.Consumers;

public sealed class UserRegisteredConsumer(
    AnalyticsDbContext db,
    ILogger<UserRegisteredConsumer> logger)
    : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var msg = context.Message;
        var today = DateOnly.FromDateTime(msg.OccurredAt.UtcDateTime);

        var summary = await db.PlatformDailySummaries
            .FirstOrDefaultAsync(x => x.Date == today, context.CancellationToken);

        if (summary is null)
        {
            summary = new PlatformDailySummary(Guid.NewGuid()) { Date = today };
            db.PlatformDailySummaries.Add(summary);
        }

        summary.NewRegistrations++;
        summary.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("UserRegistered: incremented NewRegistrations for {Date}", today);
    }
}
