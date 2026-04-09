using LocMp.Catalog.Infrastructure.Persistence;
using LocMp.Contracts.Identity;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Catalog.Infrastructure.Consumers;

public sealed class ShopProfileUpdatedConsumer(CatalogDbContext db, IDistributedCache cache)
    : IConsumer<ShopProfileUpdatedEvent>
{
    public async Task Consume(ConsumeContext<ShopProfileUpdatedEvent> context)
    {
        var msg = context.Message;

        var shop = await db.ShopReadModels.FindAsync([msg.ShopId], context.CancellationToken);
        if (shop is null)
            return;

        shop.BusinessName = msg.BusinessName;
        shop.Description = msg.Description;
        shop.WorkingHours = msg.WorkingHours;
        shop.ServiceRadiusMeters = msg.ServiceRadiusMeters;
        shop.IsActive = msg.IsActive;
        shop.LastSyncedAt = msg.OccurredAt;

        await db.SaveChangesAsync(context.CancellationToken);
        await cache.RemoveAsync($"shop:{msg.ShopId}", context.CancellationToken);
    }
}