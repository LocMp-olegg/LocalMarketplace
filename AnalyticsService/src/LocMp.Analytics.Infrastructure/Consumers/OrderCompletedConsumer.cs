using LocMp.Analytics.Domain.Entities;
using LocMp.Analytics.Infrastructure.Consumers.Helpers;
using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.Contracts.Orders;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocMp.Analytics.Infrastructure.Consumers;

public sealed class OrderCompletedConsumer(
    AnalyticsDbContext db,
    ILogger<OrderCompletedConsumer> logger)
    : IConsumer<OrderCompletedEvent>
{
    public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
    {
        var msg = context.Message;
        var now = msg.OccurredAt;
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        foreach (var periodType in PeriodHelper.All)
        {
            var periodStart = PeriodHelper.GetPeriodStart(periodType, now);

            // — SellerSalesSummary —
            var sales = await db.SellerSalesSummaries
                .FirstOrDefaultAsync(x => x.SellerId == msg.SellerId
                                          && x.PeriodType == periodType
                                          && x.PeriodStart == periodStart,
                    context.CancellationToken);

            if (sales is null)
            {
                sales = new SellerSalesSummary(Guid.NewGuid())
                {
                    SellerId = msg.SellerId,
                    PeriodType = periodType,
                    PeriodStart = periodStart
                };
                db.SellerSalesSummaries.Add(sales);
            }

            sales.TotalRevenue += msg.TotalAmount;
            sales.OrderCount++;
            sales.CompletedCount++;
            sales.AverageOrderValue = sales.TotalRevenue / sales.OrderCount;
            sales.UpdatedAt = DateTimeOffset.UtcNow;

            // — SellerLeaderboard —
            var leaderboard = await db.SellerLeaderboards
                .FirstOrDefaultAsync(x => x.SellerId == msg.SellerId
                                          && x.PeriodType == periodType
                                          && x.PeriodStart == periodStart,
                    context.CancellationToken);

            if (leaderboard is null)
            {
                leaderboard = new SellerLeaderboard(Guid.NewGuid())
                {
                    SellerId = msg.SellerId,
                    SellerName = string.Empty,
                    PeriodType = periodType,
                    PeriodStart = periodStart
                };
                db.SellerLeaderboards.Add(leaderboard);
            }

            leaderboard.TotalRevenue += msg.TotalAmount;
            leaderboard.OrderCount++;
            leaderboard.UpdatedAt = DateTimeOffset.UtcNow;

            // — TopProduct —
            foreach (var productId in msg.ProductIds)
            {
                var top = await db.TopProducts
                    .FirstOrDefaultAsync(x => x.ProductId == productId
                                              && x.SellerId == msg.SellerId
                                              && x.PeriodType == periodType
                                              && x.PeriodStart == periodStart,
                        context.CancellationToken);

                if (top is null)
                {
                    top = new TopProduct(Guid.NewGuid())
                    {
                        SellerId = msg.SellerId,
                        ProductId = productId,
                        ProductName = string.Empty,
                        PeriodType = periodType,
                        PeriodStart = periodStart
                    };
                    db.TopProducts.Add(top);
                }

                top.TotalSold++;
                top.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        // — PlatformDailySummary —
        var platform = await db.PlatformDailySummaries
            .FirstOrDefaultAsync(x => x.Date == today, context.CancellationToken);

        if (platform is null)
        {
            platform = new PlatformDailySummary(Guid.NewGuid()) { Date = today };
            db.PlatformDailySummaries.Add(platform);
        }

        platform.CompletedOrders++;
        platform.GrossMerchandiseValue += msg.TotalAmount;
        platform.AverageOrderValue = platform.TotalOrders > 0
            ? platform.GrossMerchandiseValue / platform.TotalOrders
            : msg.TotalAmount;
        platform.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("OrderCompleted: updated analytics for seller {SellerId}, order {OrderId}",
            msg.SellerId, msg.OrderId);
    }
}