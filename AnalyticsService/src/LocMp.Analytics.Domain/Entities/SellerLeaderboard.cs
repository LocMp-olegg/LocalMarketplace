using LocMp.Analytics.Domain.Enums;
using LocMp.BuildingBlocks;

namespace LocMp.Analytics.Domain.Entities;

public class SellerLeaderboard(Guid id) : Entity<Guid>(id)
{
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = null!;

    public Guid? ShopId { get; set; }
    public string? ShopName { get; set; }

    public PeriodType PeriodType { get; set; }
    public DateOnly PeriodStart { get; set; }

    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageRating { get; set; }

    public int RevenueRank { get; set; }
    public int OrderCountRank { get; set; }
    public DateTimeOffset? RanksComputedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
