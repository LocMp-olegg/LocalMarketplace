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
        var today = DateOnly.FromDateTime(msg.OccurredAt.UtcDateTime);

        if (string.Equals(msg.SubjectType, "Seller", StringComparison.OrdinalIgnoreCase))
        {
            await HandleSellerRating(msg, today, context.CancellationToken);
        }
        else if (string.Equals(msg.SubjectType, "Product", StringComparison.OrdinalIgnoreCase))
        {
            await HandleProductRating(msg, context.CancellationToken);
        }
    }

    private async Task HandleSellerRating(RatingAggregateUpdatedEvent msg, DateOnly today, CancellationToken ct)
    {
        var history = await db.SellerRatingHistory
            .FirstOrDefaultAsync(x => x.SellerId == msg.SubjectId && x.RecordedAt == today, ct);

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

        await db.SellerLeaderboards
            .Where(x => x.SellerId == msg.SubjectId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.AverageRating, msg.NewAverage), ct);

        await db.SaveChangesAsync(ct);
        logger.LogInformation("RatingUpdated[Seller]: upserted SellerRatingHistory for seller {SellerId} on {Date}",
            msg.SubjectId, today);
    }

    private async Task HandleProductRating(RatingAggregateUpdatedEvent msg, CancellationToken ct)
    {
        var summary = await db.ProductRatingSummaries
            .FirstOrDefaultAsync(x => x.ProductId == msg.SubjectId, ct);

        if (summary is null)
        {
            summary = new ProductRatingSummary(Guid.NewGuid())
            {
                ProductId   = msg.SubjectId,
                SellerId    = msg.SellerId ?? Guid.Empty,
                ProductName = string.Empty
            };
            db.ProductRatingSummaries.Add(summary);
        }
        else if (summary.SellerId == Guid.Empty && msg.SellerId.HasValue)
        {
            summary.SellerId = msg.SellerId.Value;
        }

        summary.AverageRating = msg.NewAverage;
        summary.ReviewCount   = msg.ReviewCount;
        summary.UpdatedAt     = msg.OccurredAt;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("RatingUpdated[Product]: updated ProductRatingSummary for product {ProductId}",
            msg.SubjectId);
    }
}
