using LocMp.Order.Domain.Enums;
using Xunit;
using OrderEntity = LocMp.Order.Domain.Entities.Order;

namespace LocMp.Order.Application.Tests.Domain;

public sealed class OrderTransitionTests
{
    private static OrderEntity MakeOrder(OrderStatus status = OrderStatus.Pending)
    {
        var order = new OrderEntity(Guid.NewGuid()) { BuyerId = Guid.NewGuid(), SellerId = Guid.NewGuid() };
        if (status != OrderStatus.Pending)
        {
            var now = DateTimeOffset.UtcNow;
            order.TransitionTo(OrderStatus.Confirmed, order.SellerId, now);
            if (status == OrderStatus.ReadyForPickup)
                order.TransitionTo(OrderStatus.ReadyForPickup, order.SellerId, now);
            else if (status == OrderStatus.InDelivery)
                order.TransitionTo(OrderStatus.InDelivery, order.SellerId, now);
        }
        return order;
    }

    [Fact]
    public void TransitionTo_PendingToConfirmed_Succeeds()
    {
        var order = MakeOrder();
        var now = DateTimeOffset.UtcNow;

        var (prev, history) = order.TransitionTo(OrderStatus.Confirmed, order.SellerId, now);

        Assert.Equal(OrderStatus.Pending, prev);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Equal(OrderStatus.Confirmed, history.ToStatus);
        Assert.Equal(OrderStatus.Pending, history.FromStatus);
        Assert.Equal(order.Id, history.OrderId);
    }

    [Fact]
    public void TransitionTo_PendingToCancelled_Succeeds()
    {
        var order = MakeOrder();
        var (prev, _) = order.TransitionTo(OrderStatus.Cancelled, order.BuyerId, DateTimeOffset.UtcNow);

        Assert.Equal(OrderStatus.Pending, prev);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public void TransitionTo_ConfirmedToCompleted_ThrowsInvalidOperation()
    {
        var order = MakeOrder();
        order.TransitionTo(OrderStatus.Confirmed, order.SellerId, DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            order.TransitionTo(OrderStatus.Completed, order.BuyerId, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void TransitionTo_ReadyForPickupToCompleted_SetsCompletedAt()
    {
        var order = MakeOrder(OrderStatus.ReadyForPickup);
        var now = DateTimeOffset.UtcNow;

        order.TransitionTo(OrderStatus.Completed, order.BuyerId, now);

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.NotNull(order.CompletedAt);
    }

    [Fact]
    public void TransitionTo_CompletedToAny_ThrowsInvalidOperation()
    {
        var order = MakeOrder(OrderStatus.ReadyForPickup);
        var now = DateTimeOffset.UtcNow;
        order.TransitionTo(OrderStatus.Completed, order.BuyerId, now);

        Assert.Throws<InvalidOperationException>(() =>
            order.TransitionTo(OrderStatus.Cancelled, order.BuyerId, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void TransitionTo_UpdatesUpdatedAt()
    {
        var order = MakeOrder();
        var before = DateTimeOffset.UtcNow;

        order.TransitionTo(OrderStatus.Confirmed, order.SellerId, before);

        Assert.Equal(before, order.UpdatedAt);
    }

    [Theory]
    [InlineData(OrderStatus.Confirmed, OrderStatus.ReadyForPickup)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.InDelivery)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Disputed)]
    public void TransitionTo_AllowedFromConfirmed_Succeeds(OrderStatus from, OrderStatus to)
    {
        var order = MakeOrder();
        order.TransitionTo(OrderStatus.Confirmed, order.SellerId, DateTimeOffset.UtcNow);

        var (prev, _) = order.TransitionTo(to, order.SellerId, DateTimeOffset.UtcNow);

        Assert.Equal(from, prev);
        Assert.Equal(to, order.Status);
    }
}
