using LocMp.Catalog.Infrastructure.Persistence;
using LocMp.Contracts.Review;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Catalog.Infrastructure.Consumers;

public sealed class RatingAggregateUpdatedConsumer(CatalogDbContext db, IDistributedCache cache)
    : IConsumer<RatingAggregateUpdatedEvent>
{
    public async Task Consume(ConsumeContext<RatingAggregateUpdatedEvent> context)
    {
        var msg = context.Message;
        var ct = context.CancellationToken;

        if (msg.SubjectType == "Product")
        {
            var rows = await db.Products
                .Where(p => p.Id == msg.SubjectId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.AverageRating, msg.NewAverage)
                    .SetProperty(p => p.ReviewCount, msg.ReviewCount),
                    ct);

            if (rows > 0)
            {
                await cache.RemoveAsync($"product:{msg.SubjectId}", ct);
                await RecalculateShopRatingAsync(msg.SubjectId, msg.SellerId, ct);
            }
        }
        else if (msg.SubjectType == "Seller")
        {
            var seller = await db.SellerReadModels.FindAsync([msg.SubjectId], ct);
            if (seller is null)
                return;

            seller.AverageRating = msg.NewAverage;
            seller.ReviewCount = msg.ReviewCount;
            seller.LastSyncedAt = msg.OccurredAt;

            await db.SaveChangesAsync(ct);
            await cache.RemoveAsync($"seller:{msg.SubjectId}", ct);
        }
    }

    private async Task RecalculateShopRatingAsync(Guid productId, Guid? sellerId, CancellationToken ct)
    {
        var resolvedSellerId = sellerId ?? await db.Products
            .Where(p => p.Id == productId)
            .Select(p => (Guid?)p.SellerId)
            .FirstOrDefaultAsync(ct);

        if (resolvedSellerId is null)
            return;

        var shop = await db.Shops
            .FirstOrDefaultAsync(s => s.SellerId == resolvedSellerId.Value, ct);

        if (shop is null)
            return;

        var productRatings = await db.Products
            .Where(p => p.ShopId == shop.Id && !p.IsDeleted && p.ReviewCount > 0)
            .Select(p => new { p.AverageRating, p.ReviewCount })
            .ToListAsync(ct);

        var totalReviews = productRatings.Sum(p => p.ReviewCount);
        var weightedSum = productRatings.Sum(p => p.AverageRating * p.ReviewCount);

        shop.AverageRating = totalReviews > 0
            ? Math.Round(weightedSum / totalReviews, 2)
            : 0m;
        shop.ReviewCount = totalReviews;
        shop.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync($"shop:{shop.Id}", ct);
    }
}
