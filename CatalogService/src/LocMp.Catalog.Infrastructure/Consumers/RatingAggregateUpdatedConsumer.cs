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
                await cache.RemoveAsync($"product:{msg.SubjectId}", ct);
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
}
