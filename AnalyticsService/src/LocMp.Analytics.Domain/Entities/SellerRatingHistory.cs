using LocMp.BuildingBlocks;

namespace LocMp.Analytics.Domain.Entities;

public class SellerRatingHistory(Guid id) : Entity<Guid>(id)
{
    public Guid SellerId { get; set; }
    public DateOnly RecordedAt { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int NewReviewsToday { get; set; }
}
