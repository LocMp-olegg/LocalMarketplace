using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using LocMp.Contracts.Identity;
using MassTransit;

namespace LocMp.Catalog.Infrastructure.Consumers;

public sealed class UserRegisteredConsumer(CatalogDbContext db) : IConsumer<UserRegisteredEvent>
{
    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var msg = context.Message;

        var exists = await db.SellerReadModels.FindAsync([msg.UserId]);
        if (exists is not null)
            return;

        db.SellerReadModels.Add(new SellerReadModel(msg.UserId)
        {
            DisplayName = msg.DisplayName,
            AvatarUrl = null,
            AverageRating = 0,
            ReviewCount = 0,
            LastSyncedAt = msg.OccurredAt
        });

        await db.SaveChangesAsync(context.CancellationToken);
    }
}
