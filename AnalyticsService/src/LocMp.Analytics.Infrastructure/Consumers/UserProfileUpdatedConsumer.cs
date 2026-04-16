using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.Contracts.Identity;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocMp.Analytics.Infrastructure.Consumers;

public sealed class UserProfileUpdatedConsumer(
    AnalyticsDbContext db,
    ILogger<UserProfileUpdatedConsumer> logger)
    : IConsumer<UserProfileUpdatedEvent>
{
    public async Task Consume(ConsumeContext<UserProfileUpdatedEvent> context)
    {
        var msg = context.Message;

        var updated = await db.SellerLeaderboards
            .Where(x => x.SellerId == msg.UserId)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.SellerName, msg.DisplayName),
                context.CancellationToken);

        if (updated > 0)
            logger.LogInformation("UserProfileUpdated: refreshed SellerName in {Count} leaderboard rows for seller {SellerId}",
                updated, msg.UserId);
    }
}
