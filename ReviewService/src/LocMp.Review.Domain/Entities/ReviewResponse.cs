using LocMp.BuildingBlocks;

namespace LocMp.Review.Domain.Entities;

public class ReviewResponse(Guid id) : Entity<Guid>(id)
{
    public Guid ReviewId { get; set; }
    public Guid AuthorId { get; set; }

    public string Comment { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual Review Review { get; set; } = null!;
}