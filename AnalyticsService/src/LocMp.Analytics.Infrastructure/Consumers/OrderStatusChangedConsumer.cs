using LocMp.Analytics.Domain.Entities;
using LocMp.Analytics.Infrastructure.Consumers.Helpers;
using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.Contracts.Orders;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocMp.Analytics.Infrastructure.Consumers;

public sealed class OrderStatusChangedConsumer(
    AnalyticsDbContext db,
    ILogger<OrderStatusChangedConsumer> logger)
    : IConsumer<OrderStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<OrderStatusChangedEvent> context)
    {
        var msg = context.Message;
        var isCancelled = string.Equals(msg.ToStatus, "Cancelled", StringComparison.OrdinalIgnoreCase);
        var isDisputed  = string.Equals(msg.ToStatus, "Disputed",  StringComparison.OrdinalIgnoreCase);

        if (!isCancelled && !isDisputed)
            return;

        var now   = msg.OccurredAt;
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        // — SellerSalesSummary (Daily, Weekly, Monthly) —
        foreach (var periodType in PeriodHelper.All)
        {
            var periodStart = PeriodHelper.GetPeriodStart(periodType, now);

            var summary = await db.SellerSalesSummaries
                .FirstOrDefaultAsync(x => x.SellerId == msg.SellerId
                                       && x.PeriodType == periodType
                                       && x.PeriodStart == periodStart,
                    context.CancellationToken);

            if (summary is null)
            {
                summary = new SellerSalesSummary(Guid.NewGuid())
                {
                    SellerId    = msg.SellerId,
                    PeriodType  = periodType,
                    PeriodStart = periodStart
                };
                db.SellerSalesSummaries.Add(summary);
            }

            if (isCancelled) summary.CancelledCount++;
            if (isDisputed)  summary.DisputedCount++;
            summary.UpdatedAt = DateTimeOffset.UtcNow;
        }

        // — PlatformDailySummary —
        var platform = await db.PlatformDailySummaries
            .FirstOrDefaultAsync(x => x.Date == today, context.CancellationToken);

        if (platform is null)
        {
            platform = new PlatformDailySummary(Guid.NewGuid()) { Date = today };
            db.PlatformDailySummaries.Add(platform);
        }

        if (isCancelled) platform.CancelledOrders++;
        if (isDisputed)  platform.DisputedOrders++;
        platform.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("OrderStatusChanged → {ToStatus}: updated SellerSalesSummary + PlatformDailySummary for seller {SellerId}",
            msg.ToStatus, msg.SellerId);
    }
}
