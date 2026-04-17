using LocMp.BuildingBlocks;

namespace LocMp.Order.Domain.Entities;

public class OrderItem(Guid id) : Entity<Guid>(id)
{
    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public string? ProductDescription { get; set; }
    public string? MainPhotoUrl { get; set; }
    public Guid? ShopId { get; set; }
    public string? ShopName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }

    public virtual Order Order { get; set; } = null!;
}
