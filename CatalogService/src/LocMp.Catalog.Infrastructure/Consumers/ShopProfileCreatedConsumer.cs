using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using LocMp.Contracts.Identity;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Catalog.Infrastructure.Consumers;

public sealed class ShopProfileCreatedConsumer(CatalogDbContext db, IDistributedCache cache)
    : IConsumer<ShopProfileCreatedEvent>
{
    public async Task Consume(ConsumeContext<ShopProfileCreatedEvent> context)
    {
        var msg = context.Message;

        var sellerExists = await db.SellerReadModels.AnyAsync(s => s.Id == msg.UserId, context.CancellationToken);
        if (!sellerExists)
        {
            throw new InvalidOperationException(
                $"Cannot create shop. Seller '{msg.UserId}' not found yet. Retrying...");
        }

        var shop = await db.ShopReadModels.FindAsync([msg.ShopId], context.CancellationToken);
        if (shop is null)
        {
            db.ShopReadModels.Add(new ShopReadModel(msg.ShopId)
            {
                SellerId = msg.UserId,
                BusinessName = msg.BusinessName,
                IsActive = true,
                LastSyncedAt = msg.OccurredAt
            });

            await db.SaveChangesAsync(context.CancellationToken);
            await cache.RemoveAsync($"seller:{msg.UserId}", context.CancellationToken);
        }
    }
}