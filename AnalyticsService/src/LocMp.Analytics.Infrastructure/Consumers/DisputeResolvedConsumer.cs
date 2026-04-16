using LocMp.Analytics.Domain.Entities;
using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.Contracts.Orders;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocMp.Analytics.Infrastructure.Consumers;

public sealed class DisputeResolvedConsumer(
    AnalyticsDbContext db,
    ILogger<DisputeResolvedConsumer> logger)
    : IConsumer<DisputeResolvedEvent>
{
    public async Task Consume(ConsumeContext<DisputeResolvedEvent> context)
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

        summary.ResolvedCount++;

        if (msg.Outcome == DisputeOutcome.BuyerFavored)
            summary.BuyerFavoredCount++;
        else
            summary.SellerFavoredCount++;

        var prevCount = summary.ResolvedCount - 1;
        summary.AverageResolutionMinutes =
            prevCount == 0
                ? msg.ResolutionMinutes
                : (summary.AverageResolutionMinutes * prevCount + msg.ResolutionMinutes) / summary.ResolvedCount;

        summary.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("DisputeResolved: updated DisputeSummary for {Date}, outcome={Outcome}",
            today, msg.Outcome);
    }
}
