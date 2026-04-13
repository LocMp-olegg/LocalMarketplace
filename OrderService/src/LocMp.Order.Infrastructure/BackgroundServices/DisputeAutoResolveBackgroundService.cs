using LocMp.Contracts.Dispute;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Options;
using LocMp.Order.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LocMp.Order.Infrastructure.BackgroundServices;

public sealed class DisputeAutoResolveBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<DisputeOptions> options,
    ILogger<DisputeAutoResolveBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DisputeAutoResolveBackgroundService started (threshold: {Days} days)",
            options.Value.AutoResolveDays);

        while (!stoppingToken.IsCancellationRequested)
        {
            await AutoResolveOldDisputesAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task AutoResolveOldDisputesAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            var threshold = DateTimeOffset.UtcNow.AddDays(-options.Value.AutoResolveDays);

            var disputes = await db.Disputes
                .Include(d => d.Order)
                .Where(d => d.Status == DisputeStatus.Open && d.CreatedAt <= threshold)
                .ToListAsync(ct);

            if (disputes.Count == 0) return;

            var now = DateTimeOffset.UtcNow;
            var eventsToPublish = new List<(DisputeResolvedEvent Dispute, OrderStatusChangedEvent Order)>(disputes.Count);

            foreach (var dispute in disputes)
            {
                dispute.Status = DisputeStatus.Resolved;
                dispute.Resolution = "Auto-resolved: no response within the allowed period.";
                dispute.ResolvedAt = now;

                var order = dispute.Order;
                var prev = order.Status;
                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = now;

                db.OrderStatusHistory.Add(new OrderStatusHistory(Guid.NewGuid())
                {
                    OrderId = order.Id,
                    FromStatus = prev,
                    ToStatus = OrderStatus.Cancelled,
                    Comment = "Auto-resolved dispute",
                    ChangedById = order.BuyerId,
                    ChangedAt = now
                });

                eventsToPublish.Add((
                    new DisputeResolvedEvent(dispute.Id, order.Id, options.Value.AutoResolveDays * 24 * 60, now),
                    new OrderStatusChangedEvent(order.Id, order.BuyerId, order.SellerId,
                        prev.ToString(), nameof(OrderStatus.Cancelled), now)));
            }

            await db.SaveChangesAsync(ct);

            foreach (var (disputeEvt, orderEvt) in eventsToPublish)
            {
                await publishEndpoint.Publish(disputeEvt, ct);
                await publishEndpoint.Publish(orderEvt, ct);
            }

            logger.LogInformation("Auto-resolved {Count} stale disputes", disputes.Count);
        }
        catch (OperationCanceledException) { /* shutdown */ }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during dispute auto-resolve");
        }
    }
}
