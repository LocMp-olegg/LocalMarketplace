using LocMp.Contracts.Identity;
using LocMp.Identity.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LocMp.Identity.Infrastructure.BackgroundServices;

public class UserUnblockingBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<UserUnblockingBackgroundService> logger) : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Background unblocking service started with interval {Interval}ms",
            _checkInterval.TotalMilliseconds);

        using var timer = new PeriodicTimer(_checkInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await UnblockExpiredUsersAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Critical error during background user unblocking");
            }
        }
    }

    private async Task UnblockExpiredUsersAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var expiredUsers = await context.Users
            .Where(u => u.LockoutEnd != null && u.LockoutEnd <= DateTimeOffset.UtcNow)
            .Select(u => u.Id)
            .ToListAsync(ct);

        if (expiredUsers.Count == 0)
            return;

        await context.Users
            .Where(u => expiredUsers.Contains(u.Id))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.LockoutEnd, (DateTimeOffset?)null)
                .SetProperty(u => u.Active, true)
                .SetProperty(u => u.AccessFailedCount, 0), ct);

        var now = DateTimeOffset.UtcNow;
        foreach (var userId in expiredUsers)
            await publishEndpoint.Publish(new UserUnblockedEvent(userId, now), ct);

        logger.LogInformation("Batch unblock completed. {UserCount} users were reactivated", expiredUsers.Count);
    }
}