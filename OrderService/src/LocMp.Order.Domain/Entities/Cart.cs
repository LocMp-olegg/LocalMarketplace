using LocMp.BuildingBlocks;

namespace LocMp.Order.Domain.Entities;

public class Cart(Guid id) : Entity<Guid>(id)
{
    public Guid UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }

    public virtual ICollection<CartItem> Items { get; set; } = [];
}