using LocMp.Analytics.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LocMp.Analytics.Infrastructure.BackgroundServices;

/// <summary>
/// Сбрасывает ViewsToday в полночь и ViewsThisWeek в понедельник.
/// </summary>
public sealed class ViewCountResetBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<ViewCountResetBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var nextMidnight = now.Date.AddDays(1);
            var delay = nextMidnight - now.DateTime;

            await Task.Delay(delay, stoppingToken);

            await ResetDailyCountersAsync(stoppingToken);

            if (DateTimeOffset.UtcNow.DayOfWeek == DayOfWeek.Monday)
                await ResetWeeklyCountersAsync(stoppingToken);
        }
    }

    private async Task ResetDailyCountersAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

            await db.ProductViewCounters
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.ViewsToday, 0), ct);

            logger.LogInformation("ViewsToday reset for all products");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resetting ViewsToday");
        }
    }

    private async Task ResetWeeklyCountersAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

            await db.ProductViewCounters
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.ViewsThisWeek, 0), ct);

            logger.LogInformation("ViewsThisWeek reset for all products");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resetting ViewsThisWeek");
        }
    }
}