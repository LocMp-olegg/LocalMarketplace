using LocMp.Analytics.Domain.Entities;
using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.Contracts.Orders;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LocMp.Analytics.Infrastructure.Consumers;

public sealed class OrderPlacedConsumer(
    AnalyticsDbContext db,
    IConnectionMultiplexer redis,
    ILogger<OrderPlacedConsumer> logger)
    : IConsumer<OrderPlacedEvent>
{
    private static readonly TimeSpan KeyTtl = TimeSpan.FromHours(48);

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var msg   = context.Message;
        var today = DateOnly.FromDateTime(msg.OccurredAt.UtcDateTime);
        var dateStr = today.ToString("yyyy-MM-dd");

        // — Redis Sets: уникальные участники за день —
        var redisDb     = redis.GetDatabase();
        var buyerKey    = $"analytics:active-buyers:{dateStr}";
        var sellerKey   = $"analytics:active-sellers:{dateStr}";

        await redisDb.SetAddAsync(buyerKey,  msg.BuyerId.ToString());
        await redisDb.SetAddAsync(sellerKey, msg.SellerId.ToString());
        await redisDb.KeyExpireAsync(buyerKey,  KeyTtl);
        await redisDb.KeyExpireAsync(sellerKey, KeyTtl);

        var activeBuyers  = (int)await redisDb.SetLengthAsync(buyerKey);
        var activeSellers = (int)await redisDb.SetLengthAsync(sellerKey);

        // — PlatformDailySummary —
        var summary = await db.PlatformDailySummaries
            .FirstOrDefaultAsync(x => x.Date == today, context.CancellationToken);

        if (summary is null)
        {
            summary = new PlatformDailySummary(Guid.NewGuid()) { Date = today };
            db.PlatformDailySummaries.Add(summary);
        }

        summary.TotalOrders++;
        summary.ActiveBuyers  = activeBuyers;
        summary.ActiveSellers = activeSellers;
        summary.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation(
            "OrderPlaced: TotalOrders={Total}, ActiveBuyers={Buyers}, ActiveSellers={Sellers} for {Date}",
            summary.TotalOrders, activeBuyers, activeSellers, today);
    }
}
