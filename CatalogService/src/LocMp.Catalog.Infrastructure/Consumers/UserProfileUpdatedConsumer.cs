using LocMp.Catalog.Infrastructure.Persistence;
using LocMp.Contracts.Identity;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Catalog.Infrastructure.Consumers;

public sealed class UserProfileUpdatedConsumer(CatalogDbContext db, IDistributedCache cache)
    : IConsumer<UserProfileUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserProfileUpdatedEvent> context)
    {
        var msg = context.Message;

        var seller = await db.SellerReadModels.FindAsync([msg.UserId]);
        if (seller is null)
            return;

        seller.DisplayName = msg.DisplayName;
        if (msg.AvatarUrl is not null)
            seller.AvatarUrl = msg.AvatarUrl;
        seller.LastSyncedAt = msg.OccurredAt;

        await db.SaveChangesAsync(context.CancellationToken);
        await cache.RemoveAsync($"seller:{msg.UserId}", context.CancellationToken);
    }
}
