using LocMp.Catalog.Infrastructure.Persistence;
using LocMp.Contracts.Identity;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace LocMp.Catalog.Infrastructure.Consumers;

public sealed class UserLostSellerStatusConsumer(CatalogDbContext db, IConnectionMultiplexer redis)
    : IConsumer<UserLostSellerStatusEvent>
{
    public async Task Consume(ConsumeContext<UserLostSellerStatusEvent> context)
    {
        var ct = context.CancellationToken;

        var shopIds = await db.Shops
            .Where(s => s.SellerId == context.Message.UserId && s.IsActive)
            .Select(s => s.Id)
            .ToListAsync(ct);

        if (shopIds.Count == 0)
            return;

        await db.Shops
            .Where(s => shopIds.Contains(s.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.IsActive, false)
                .SetProperty(p => p.UpdatedAt, context.Message.OccurredAt), ct);

        var productIds = await db.Products
            .Where(p => shopIds.Contains(p.ShopId) && !p.IsDeleted)
            .Select(p => p.Id)
            .ToListAsync(ct);

        await InvalidateCachesAsync(productIds);
    }

    private async Task InvalidateCachesAsync(IReadOnlyList<Guid> productIds)
    {
        var server = redis.GetServer(redis.GetEndPoints()[0]);
        var database = redis.GetDatabase();

        var listPatterns = new[] { "locmp-catalog:products:search:*", "locmp-catalog:products:location:*" };
        foreach (var pattern in listPatterns)
        {
            var keys = server.Keys(pattern: pattern).ToArray();
            if (keys.Length > 0)
                await database.KeyDeleteAsync(keys);
        }

        if (productIds.Count > 0)
        {
            var productKeys = productIds
                .Select(id => new RedisKey($"locmp-catalog:product:{id}"))
                .ToArray();
            await database.KeyDeleteAsync(productKeys);
        }
    }
}
