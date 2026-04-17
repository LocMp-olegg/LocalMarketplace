using LocMp.BuildingBlocks;
using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Domain.Entities;

public class Order(Guid id) : AggregateRoot<Guid>(id)
{
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> AllowedTransitions = new()
    {
        [OrderStatus.Pending] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] =
            [OrderStatus.ReadyForPickup, OrderStatus.InDelivery, OrderStatus.Cancelled, OrderStatus.Disputed],
        [OrderStatus.ReadyForPickup] = [OrderStatus.Completed, OrderStatus.Disputed],
        [OrderStatus.InDelivery] = [OrderStatus.Completed, OrderStatus.Disputed],
        [OrderStatus.Disputed] = [OrderStatus.Cancelled, OrderStatus.Completed],
    };

    public Guid BuyerId { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;

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

    public (OrderStatus Prev, OrderStatusHistory Entry) TransitionTo(
        OrderStatus to, Guid changedById, DateTimeOffset now, string? comment = null)
    {
        if (!AllowedTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(to))
            throw new InvalidOperationException($"Cannot transition order from '{Status}' to '{to}'.");

        var prev = Status;
        Status = to;
        UpdatedAt = now;

        if (to == OrderStatus.Completed)
            CompletedAt = now;

        return (prev, new OrderStatusHistory(Guid.NewGuid())
        {
            OrderId = Id,
            FromStatus = prev,
            ToStatus = to,
            Comment = comment,
            ChangedById = changedById,
            ChangedAt = now
        });
    }
}