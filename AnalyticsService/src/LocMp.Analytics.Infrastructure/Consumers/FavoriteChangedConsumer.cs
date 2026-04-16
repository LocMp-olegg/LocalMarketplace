using LocMp.Analytics.Domain.Entities;
using LocMp.Analytics.Infrastructure.Consumers.Helpers;
using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.Contracts.Catalog;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocMp.Analytics.Infrastructure.Consumers;

public sealed class FavoriteChangedConsumer(
    AnalyticsDbContext db,
    ILogger<FavoriteChangedConsumer> logger)
    : IConsumer<FavoriteChangedEvent>
{
    public async Task Consume(ConsumeContext<FavoriteChangedEvent> context)
    {
        var msg = context.Message;
        var now = msg.OccurredAt;

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

            if (msg.IsAdded)
                top.FavoriteCount++;
            else if (top.FavoriteCount > 0)
                top.FavoriteCount--;

            top.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await db.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("FavoriteChanged: updated TopProduct.FavoriteCount for product {ProductId} (isAdded={IsAdded})",
            msg.ProductId, msg.IsAdded);
    }
}
