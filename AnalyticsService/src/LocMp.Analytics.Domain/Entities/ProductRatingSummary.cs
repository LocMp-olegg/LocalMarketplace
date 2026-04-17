using LocMp.BuildingBlocks;

namespace LocMp.Analytics.Domain.Entities;

public class ProductRatingSummary(Guid id) : Entity<Guid>(id)
{
    public Guid ProductId { get; set; }
    public Guid SellerId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid? ShopId { get; set; }
    public string? ShopName { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
