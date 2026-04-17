using LocMp.Analytics.Domain.Enums;
using LocMp.BuildingBlocks;

namespace LocMp.Analytics.Domain.Entities;

public class TopProduct(Guid id) : Entity<Guid>(id)
{
    public Guid SellerId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!; // snapshot

    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int ViewCount { get; set; }
    public int FavoriteCount { get; set; }

    public DateOnly PeriodStart { get; set; }
    public PeriodType PeriodType { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
