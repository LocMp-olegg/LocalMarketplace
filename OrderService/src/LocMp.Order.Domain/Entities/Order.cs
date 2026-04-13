using LocMp.BuildingBlocks;
using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Domain.Entities;

public class Order(Guid id) : AggregateRoot<Guid>(id)
{
    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DeliveryType DeliveryType { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public decimal TotalAmount { get; set; }
    public string? BuyerComment { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public virtual ICollection<OrderItem> Items { get; set; } = [];
    public virtual ICollection<OrderStatusHistory> StatusHistory { get; set; } = [];
    public virtual ICollection<OrderPhoto> Photos { get; set; } = [];
    public virtual DeliveryAddress? DeliveryAddress { get; set; }
    public virtual CourierAssignment? CourierAssignment { get; set; }
    public virtual Dispute? Dispute { get; set; }
}
