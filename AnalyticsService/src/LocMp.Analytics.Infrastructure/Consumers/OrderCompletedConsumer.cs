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

        var shopGroups = msg.Products
            .Where(p => p.ShopId.HasValue)
            .GroupBy(p => new { p.ShopId, p.ShopName })
            .ToList();

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

            // — SellerLeaderboard: seller-level aggregate (ShopId = null) —
            var leaderboard = await db.SellerLeaderboards
                .FirstOrDefaultAsync(x => x.SellerId == msg.SellerId
                                          && x.ShopId == null
                                          && x.PeriodType == periodType
                                          && x.PeriodStart == periodStart,
                    context.CancellationToken);

            if (leaderboard is null)
            {
                leaderboard = new SellerLeaderboard(Guid.NewGuid())
                {
                    SellerId = msg.SellerId,
                    SellerName = msg.SellerName,
                    ShopId = null,
                    ShopName = null,
                    PeriodType = periodType,
                    PeriodStart = periodStart
                };
                db.SellerLeaderboards.Add(leaderboard);
            }
            else if (string.IsNullOrEmpty(leaderboard.SellerName))
            {
                leaderboard.SellerName = msg.SellerName;
            }

            leaderboard.TotalRevenue += msg.TotalAmount;
            leaderboard.OrderCount++;
            leaderboard.UpdatedAt = DateTimeOffset.UtcNow;

            // — SellerLeaderboard: shop-level aggregates —
            foreach (var shopGroup in shopGroups)
            {
                var shopRevenue = shopGroup.Sum(p => p.Subtotal);
                var shopLeaderboard = await db.SellerLeaderboards
                    .FirstOrDefaultAsync(x => x.SellerId == msg.SellerId
                                              && x.ShopId == shopGroup.Key.ShopId
                                              && x.PeriodType == periodType
                                              && x.PeriodStart == periodStart,
                        context.CancellationToken);

                if (shopLeaderboard is null)
                {
                    shopLeaderboard = new SellerLeaderboard(Guid.NewGuid())
                    {
                        SellerId = msg.SellerId,
                        SellerName = msg.SellerName,
                        ShopId = shopGroup.Key.ShopId,
                        ShopName = shopGroup.Key.ShopName,
                        PeriodType = periodType,
                        PeriodStart = periodStart
                    };
                    db.SellerLeaderboards.Add(shopLeaderboard);
                }

                shopLeaderboard.TotalRevenue += shopRevenue;
                shopLeaderboard.OrderCount++;
                shopLeaderboard.UpdatedAt = DateTimeOffset.UtcNow;
            }

            // — TopProduct —
            foreach (var item in msg.Products)
            {
                var top = await db.TopProducts
                    .FirstOrDefaultAsync(x => x.ProductId == item.ProductId
                                              && x.SellerId == msg.SellerId
                                              && x.PeriodType == periodType
                                              && x.PeriodStart == periodStart,
                        context.CancellationToken);

                if (top is null)
                {
                    top = new TopProduct(Guid.NewGuid())
                    {
                        SellerId = msg.SellerId,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        PeriodType = periodType,
                        PeriodStart = periodStart
                    };
                    db.TopProducts.Add(top);
                }
                else if (string.IsNullOrEmpty(top.ProductName))
                {
                    top.ProductName = item.ProductName;
                }

                top.TotalSold += item.Quantity;
                top.TotalRevenue += item.Subtotal;
                top.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        // — Backfill ProductRatingSummary: fill empty fields from order item snapshots —
        foreach (var item in msg.Products)
        {
            if (!string.IsNullOrEmpty(item.ProductName))
                await db.ProductRatingSummaries
                    .Where(x => x.ProductId == item.ProductId && x.ProductName == string.Empty)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.ProductName, item.ProductName),
                        context.CancellationToken);

            await db.ProductRatingSummaries
                .Where(x => x.ProductId == item.ProductId && x.SellerId == Guid.Empty)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.SellerId, msg.SellerId),
                    context.CancellationToken);

            if (item.ShopId.HasValue)
                await db.ProductRatingSummaries
                    .Where(x => x.ProductId == item.ProductId && x.ShopId == null)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.ShopId, item.ShopId)
                        .SetProperty(x => x.ShopName, item.ShopName),
                        context.CancellationToken);
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