using LocMp.BuildingBlocks;

namespace LocMp.Analytics.Domain.Entities;

public class DisputeSummary(Guid id) : Entity<Guid>(id)
{
    public DateOnly Date { get; set; }
    public int OpenedCount { get; set; }
    public int ResolvedCount { get; set; }
    public int BuyerFavoredCount { get; set; }
    public int SellerFavoredCount { get; set; }
    public decimal AverageResolutionMinutes { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
