using LocMp.BuildingBlocks;

namespace LocMp.Analytics.Domain.Entities;

public class PlatformDailySummary(Guid id) : Entity<Guid>(id)
{
    public DateOnly Date { get; set; }

    public int NewRegistrations { get; set; }
    public int ActiveBuyers { get; set; }
    public int ActiveSellers { get; set; }
    public int BlockedUsers { get; set; }

    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int DisputedOrders { get; set; }

    public decimal GrossMerchandiseValue { get; set; }
    public decimal AverageOrderValue { get; set; }

    public int NewProducts { get; set; }
    public int NewReviews { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
