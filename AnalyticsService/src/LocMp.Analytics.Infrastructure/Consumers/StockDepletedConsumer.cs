using LocMp.Analytics.Domain.Entities;
using LocMp.Analytics.Domain.Enums;
using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.Contracts.Catalog;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LocMp.Analytics.Infrastructure.Consumers;

public sealed class StockDepletedConsumer(
    AnalyticsDbContext db,
    ILogger<StockDepletedConsumer> logger)
    : IConsumer<StockDepletedEvent>
{
    public async Task Consume(ConsumeContext<StockDepletedEvent> context)
    {
        var msg = context.Message;

        var alert = new StockAlert(Guid.NewGuid())
        {
            SellerId     = msg.SellerId,
            ProductId    = msg.ProductId,
            ProductName  = msg.ProductName,
            CurrentStock = 0,
            AlertType    = StockAlertType.OutOfStock,
            IsAcknowledged = false,
            CreatedAt    = msg.OccurredAt
        };

        db.StockAlerts.Add(alert);
        await db.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("StockDepleted: created StockAlert for product {ProductId}", msg.ProductId);
    }
}
