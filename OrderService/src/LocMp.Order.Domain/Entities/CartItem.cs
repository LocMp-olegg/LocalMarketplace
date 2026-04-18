using LocMp.BuildingBlocks;

namespace LocMp.Order.Domain.Entities;

public class CartItem(Guid id) : Entity<Guid>(id)
{
    public Guid CartId { get; set; }

    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public Guid? ShopId { get; set; }
    public string? ShopName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual Cart Cart { get; set; } = null!;
}
