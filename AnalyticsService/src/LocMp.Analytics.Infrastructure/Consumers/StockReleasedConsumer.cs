using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.Contracts.Catalog;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LocMp.Analytics.Infrastructure.Consumers;

public sealed class StockReleasedConsumer(
    AnalyticsDbContext db,
    ILogger<StockReleasedConsumer> logger)
    : IConsumer<StockReleasedEvent>
{
    public async Task Consume(ConsumeContext<StockReleasedEvent> context)
    {
        var msg = context.Message;

        if (msg.NewQuantity <= 0)
            return;

        await db.StockAlerts
            .Where(x => x.ProductId == msg.ProductId && !x.IsAcknowledged)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.IsAcknowledged, true)
                .SetProperty(x => x.AcknowledgedAt, msg.OccurredAt),
                context.CancellationToken);

        logger.LogInformation(
            "StockReleased: resolved alerts for product {ProductId}, newQty={NewQuantity}",
            msg.ProductId, msg.NewQuantity);
    }
}
