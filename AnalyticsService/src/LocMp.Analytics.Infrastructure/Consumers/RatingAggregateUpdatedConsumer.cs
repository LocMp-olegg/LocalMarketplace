using LocMp.Analytics.Domain.Entities;
using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.Contracts.Review;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocMp.Analytics.Infrastructure.Consumers;

public sealed class RatingAggregateUpdatedConsumer(
    AnalyticsDbContext db,
    ILogger<RatingAggregateUpdatedConsumer> logger)
    : IConsumer<RatingAggregateUpdatedEvent>
{
    public async Task Consume(ConsumeContext<RatingAggregateUpdatedEvent> context)
    {
        var msg = context.Message;

        if (!string.Equals(msg.SubjectType, "Seller", StringComparison.OrdinalIgnoreCase))
            return;

        var today = DateOnly.FromDateTime(msg.OccurredAt.UtcDateTime);

        // — SellerRatingHistory —
        var history = await db.SellerRatingHistory
            .FirstOrDefaultAsync(x => x.SellerId == msg.SubjectId && x.RecordedAt == today,
                context.CancellationToken);

        if (history is null)
        {
            history = new SellerRatingHistory(Guid.NewGuid())
            {
                SellerId   = msg.SubjectId,
                RecordedAt = today
            };
            db.SellerRatingHistory.Add(history);
        }

        history.AverageRating = msg.NewAverage;
        history.ReviewCount   = msg.ReviewCount;
        history.NewReviewsToday++;

        // — SellerLeaderboard.AverageRating —
        await db.SellerLeaderboards
            .Where(x => x.SellerId == msg.SubjectId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.AverageRating, msg.NewAverage),
                context.CancellationToken);

        await db.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("RatingUpdated: upserted SellerRatingHistory and refreshed SellerLeaderboard for seller {SellerId} on {Date}",
            msg.SubjectId, today);
    }
}
