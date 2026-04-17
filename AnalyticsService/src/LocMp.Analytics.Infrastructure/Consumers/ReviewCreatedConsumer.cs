using LocMp.Analytics.Domain.Entities;
using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.Contracts.Review;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocMp.Analytics.Infrastructure.Consumers;

public sealed class ReviewCreatedConsumer(
    AnalyticsDbContext db,
    ILogger<ReviewCreatedConsumer> logger)
    : IConsumer<ReviewCreatedEvent>
{
    public async Task Consume(ConsumeContext<ReviewCreatedEvent> context)
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

        summary.NewReviews++;
        summary.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("ReviewCreated: incremented NewReviews for {Date}", today);
    }
}
