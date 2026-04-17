using LocMp.Analytics.Domain.Entities;
using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.Contracts.Orders;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocMp.Analytics.Infrastructure.Consumers;

public sealed class DisputeOpenedConsumer(
    AnalyticsDbContext db,
    ILogger<DisputeOpenedConsumer> logger)
    : IConsumer<DisputeOpenedEvent>
{
    public async Task Consume(ConsumeContext<DisputeOpenedEvent> context)
    {
        var msg = context.Message;
        var today = DateOnly.FromDateTime(msg.OccurredAt.UtcDateTime);

        var summary = await db.DisputeSummaries
            .FirstOrDefaultAsync(x => x.Date == today, context.CancellationToken);

        if (summary is null)
        {
            summary = new DisputeSummary(Guid.NewGuid()) { Date = today };
            db.DisputeSummaries.Add(summary);
        }

        summary.OpenedCount++;
        summary.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("DisputeOpened: incremented OpenedCount for {Date}", today);
    }
}
