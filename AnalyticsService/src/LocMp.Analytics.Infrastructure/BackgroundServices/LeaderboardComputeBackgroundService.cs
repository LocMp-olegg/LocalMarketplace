using LocMp.Analytics.Domain.Enums;
using LocMp.Analytics.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LocMp.Analytics.Infrastructure.BackgroundServices;

/// <summary>
/// Пересчитывает ранги в SellerLeaderboard каждые 6 часов.
/// </summary>
public sealed class LeaderboardComputeBackgroundService(
    IServiceScopeFactory scopeFactory,
    ILogger<LeaderboardComputeBackgroundService> logger)
    : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await ComputeRanksAsync(stoppingToken);
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ComputeRanksAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

            var now = DateTimeOffset.UtcNow;

            foreach (var periodType in Enum.GetValues<PeriodType>())
            {
                var periodStart = GetCurrentPeriodStart(periodType, now);

                var entries = await db.SellerLeaderboards
                    .Where(x => x.PeriodType == periodType && x.PeriodStart == periodStart)
                    .OrderByDescending(x => x.TotalRevenue)
                    .ToListAsync(ct);

                for (var i = 0; i < entries.Count; i++)
                    entries[i].RevenueRank = i + 1;

                var byOrderCount = entries.OrderByDescending(x => x.OrderCount).ToList();
                for (var i = 0; i < byOrderCount.Count; i++)
                    byOrderCount[i].OrderCountRank = i + 1;

                foreach (var entry in entries)
                    entry.RanksComputedAt = now;
            }

            await db.SaveChangesAsync(ct);

            logger.LogInformation("Leaderboard ranks recomputed at {Time}", now);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error computing leaderboard ranks");
        }
    }

    private static DateOnly GetCurrentPeriodStart(PeriodType periodType, DateTimeOffset now) =>
        periodType switch
        {
            PeriodType.Daily => DateOnly.FromDateTime(now.Date),
            PeriodType.Weekly => DateOnly.FromDateTime(now.Date.AddDays(-(int)now.DayOfWeek + 1)),
            PeriodType.Monthly => new DateOnly(now.Year, now.Month, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(periodType))
        };
}