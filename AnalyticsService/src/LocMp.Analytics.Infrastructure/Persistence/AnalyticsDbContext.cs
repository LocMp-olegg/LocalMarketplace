using LocMp.Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Analytics.Infrastructure.Persistence;

public class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    public DbSet<SellerSalesSummary> SellerSalesSummaries => Set<SellerSalesSummary>();
    public DbSet<TopProduct> TopProducts => Set<TopProduct>();
    public DbSet<SellerRatingHistory> SellerRatingHistory => Set<SellerRatingHistory>();
    public DbSet<StockAlert> StockAlerts => Set<StockAlert>();
    public DbSet<ProductViewCounter> ProductViewCounters => Set<ProductViewCounter>();
    public DbSet<PlatformDailySummary> PlatformDailySummaries => Set<PlatformDailySummary>();
    public DbSet<SellerLeaderboard> SellerLeaderboards => Set<SellerLeaderboard>();
    public DbSet<DisputeSummary> DisputeSummaries => Set<DisputeSummary>();
    public DbSet<GeographicActivity> GeographicActivities => Set<GeographicActivity>();
    public DbSet<ProductRatingSummary> ProductRatingSummaries => Set<ProductRatingSummary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("analytics");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
