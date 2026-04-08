using LocMp.BuildingBlocks;

namespace LocMp.Catalog.Domain.Entities;

public class Favorite(Guid id) : Entity<Guid>(id)
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public virtual Product Product { get; set; } = null!;
}