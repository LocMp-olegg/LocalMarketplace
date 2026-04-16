using LocMp.Analytics.Domain.Entities;
using LocMp.Analytics.Infrastructure.Consumers.Helpers;
using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.Contracts.Catalog;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocMp.Analytics.Infrastructure.Consumers;

public sealed class ProductViewedConsumer(
    AnalyticsDbContext db,
    ILogger<ProductViewedConsumer> logger)
    : IConsumer<ProductViewedEvent>
{
    public async Task Consume(ConsumeContext<ProductViewedEvent> context)
    {
        var msg = context.Message;
        var now = msg.OccurredAt;

        // — ProductViewCounter (PK = ProductId) —
        var counter = await db.ProductViewCounters
            .FirstOrDefaultAsync(x => x.ProductId == msg.ProductId, context.CancellationToken);

        if (counter is null)
        {
            counter = new ProductViewCounter
            {
                ProductId = msg.ProductId,
                SellerId  = msg.SellerId
            };
            db.ProductViewCounters.Add(counter);
        }

        counter.TotalViews++;
        counter.ViewsToday++;
        counter.ViewsThisWeek++;
        counter.LastViewedAt = now;

        // — TopProduct.ViewCount (Daily, Weekly, Monthly) —
        foreach (var periodType in PeriodHelper.All)
        {
            var periodStart = PeriodHelper.GetPeriodStart(periodType, now);

            var top = await db.TopProducts
                .FirstOrDefaultAsync(x => x.ProductId == msg.ProductId
                                       && x.SellerId == msg.SellerId
                                       && x.PeriodType == periodType
                                       && x.PeriodStart == periodStart,
                    context.CancellationToken);

            if (top is null)
            {
                top = new TopProduct(Guid.NewGuid())
                {
                    ProductId   = msg.ProductId,
                    SellerId    = msg.SellerId,
                    ProductName = string.Empty,
                    PeriodType  = periodType,
                    PeriodStart = periodStart
                };
                db.TopProducts.Add(top);
            }

            top.ViewCount++;
            top.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("ProductViewed: updated counters for product {ProductId}", msg.ProductId);
    }
}
