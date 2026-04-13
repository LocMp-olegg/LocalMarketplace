using LocMp.Order.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LocMp.Order.Infrastructure.BackgroundServices;

public sealed class CartCleanupBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<CartCleanupBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("CartCleanupBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupExpiredCartsAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task CleanupExpiredCartsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

            var expired = await db.Carts
                .Where(c => c.ExpiresAt <= DateTimeOffset.UtcNow)
                .ToListAsync(ct);

            if (expired.Count == 0) return;

            db.Carts.RemoveRange(expired);
            await db.SaveChangesAsync(ct);

            logger.LogInformation("Removed {Count} expired carts", expired.Count);
        }
        catch (OperationCanceledException) { /* shutdown */ }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during cart cleanup");
        }
    }
}
