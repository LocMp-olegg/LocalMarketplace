using LocMp.Analytics.Domain.Entities;
using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.Contracts.Catalog;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocMp.Analytics.Infrastructure.Consumers;

public sealed class ProductCreatedConsumer(
    AnalyticsDbContext db,
    ILogger<ProductCreatedConsumer> logger)
    : IConsumer<ProductCreatedEvent>
{
    public async Task Consume(ConsumeContext<ProductCreatedEvent> context)
    {
        var msg = context.Message;
        var today = DateOnly.FromDateTime(msg.OccurredAt.UtcDateTime);

        var summary = await db.PlatformDailySummaries
            .FirstOrDefaultAsync(x => x.Date == today, context.CancellationToken);

        if (summary is null)
        {
            summary = new PlatformDailySummary(Guid.NewGuid()) { Date = today };
            db.PlatformDailySummaries.Add(summary);
        }

        summary.NewProducts++;
        summary.UpdatedAt = DateTimeOffset.UtcNow;

        // Backfill names for TopProduct records created before ProductCreatedEvent was received
        await db.TopProducts
            .Where(x => x.ProductId == msg.ProductId && x.ProductName == string.Empty)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.ProductName, msg.ProductName),
                context.CancellationToken);

        // Upsert ProductRatingSummary: create if missing, fill empty fields if already exists
        var ratingSummary = await db.ProductRatingSummaries
            .FirstOrDefaultAsync(x => x.ProductId == msg.ProductId, context.CancellationToken);

        if (ratingSummary is null)
        {
            db.ProductRatingSummaries.Add(new ProductRatingSummary(Guid.NewGuid())
            {
                ProductId = msg.ProductId,
                SellerId = msg.SellerId,
                ProductName = msg.ProductName,
                ShopId = msg.ShopId,
                ShopName = msg.ShopName,
                UpdatedAt = msg.OccurredAt
            });
        }
        else
        {
            if (string.IsNullOrEmpty(ratingSummary.ProductName)) ratingSummary.ProductName = msg.ProductName;
            if (ratingSummary.SellerId == Guid.Empty) ratingSummary.SellerId = msg.SellerId;
            if (ratingSummary.ShopId == null && msg.ShopId.HasValue)
            {
                ratingSummary.ShopId = msg.ShopId;
                ratingSummary.ShopName = msg.ShopName;
            }
        }

        await db.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("ProductCreated: incremented NewProducts for {Date}", today);
    }
}
