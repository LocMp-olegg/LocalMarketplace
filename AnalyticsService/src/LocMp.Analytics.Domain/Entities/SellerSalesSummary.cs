using LocMp.Analytics.Domain.Enums;
using LocMp.BuildingBlocks;

namespace LocMp.Analytics.Domain.Entities;

public class SellerSalesSummary(Guid id) : Entity<Guid>(id)
{
    public Guid SellerId { get; set; }

    public PeriodType PeriodType { get; set; }
    public DateOnly PeriodStart { get; set; }

    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }

    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
    public int DisputedCount { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
