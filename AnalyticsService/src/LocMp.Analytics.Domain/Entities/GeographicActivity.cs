using LocMp.Analytics.Domain.Enums;
using LocMp.BuildingBlocks;

namespace LocMp.Analytics.Domain.Entities;

public class GeographicActivity(Guid id) : Entity<Guid>(id)
{
    public Guid ComplexId { get; set; }
    public string ComplexName { get; set; } = null!;

    public PeriodType PeriodType { get; set; }
    public DateOnly PeriodStart { get; set; }

    public int ActiveSellers { get; set; }
    public int ActiveBuyers { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
