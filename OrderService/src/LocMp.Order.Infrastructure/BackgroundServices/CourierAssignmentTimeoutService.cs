using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LocMp.Order.Infrastructure.BackgroundServices;

public sealed class CourierAssignmentTimeoutService(
    IServiceScopeFactory scopeFactory,
    ILogger<CourierAssignmentTimeoutService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan PickupTimeout = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CourierAssignmentTimeoutService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessExpiredAssignmentsAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessExpiredAssignmentsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
            var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();

            var cutoff = DateTimeOffset.UtcNow - PickupTimeout;

            var expiredAssignments = await db.CourierAssignments
                .Include(ca => ca.Order)
                .ThenInclude(o => o.CourierApplications)
                .Where(ca =>
                    ca.PickedUpAt == null &&
                    ca.AssignedAt < cutoff &&
                    ca.Order.Status == OrderStatus.Confirmed)
                .ToListAsync(ct);

            if (expiredAssignments.Count == 0) return;

            var now = DateTimeOffset.UtcNow;

            foreach (var assignment in expiredAssignments)
            {
                var order = assignment.Order;

                foreach (var app in order.CourierApplications
                             .Where(a => a.Status == CourierApplicationStatus.Approved))
                {
                    app.Status = CourierApplicationStatus.Pending;
                    app.UpdatedAt = now;
                }

                db.CourierAssignments.Remove(assignment);

                await db.SaveChangesAsync(ct);

                await eventBus.PublishAsync(new CourierAssignmentExpiredEvent(
                    order.Id, assignment.CourierId, order.SellerId, now), ct);

                logger.LogInformation(
                    "Expired courier assignment for order {OrderId}, courier {CourierId}",
                    order.Id, assignment.CourierId);
            }
        }
        catch (OperationCanceledException)
        {
            /* shutdown */
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during courier assignment timeout check");
        }
    }
}