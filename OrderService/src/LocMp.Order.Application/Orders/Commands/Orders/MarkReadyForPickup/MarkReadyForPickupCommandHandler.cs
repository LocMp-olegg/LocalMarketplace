using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;

namespace LocMp.Order.Application.Orders.Commands.Orders.MarkReadyForPickup;

public sealed class MarkReadyForPickupCommandHandler(OrderDbContext db, IEventBus eventBus)
    : IRequestHandler<MarkReadyForPickupCommand>
{
    public async Task Handle(MarkReadyForPickupCommand request, CancellationToken ct)
    {
        var order = await db.Orders.FindAsync([request.OrderId], ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (order.SellerId != request.SellerId)
            throw new ForbiddenException("You can only update your own orders.");

        if (order.Status != OrderStatus.Confirmed || order.DeliveryType != DeliveryType.Pickup)
            throw new ConflictException("Order must be Confirmed with Pickup delivery type.");

        var now = DateTimeOffset.UtcNow;
        var prev = order.Status;
        order.Status = OrderStatus.ReadyForPickup;
        order.UpdatedAt = now;

        db.OrderStatusHistory.Add(new OrderStatusHistory(Guid.NewGuid())
        {
            OrderId = order.Id,
            FromStatus = prev,
            ToStatus = OrderStatus.ReadyForPickup,
            ChangedById = request.SellerId,
            ChangedAt = now
        });

        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new OrderStatusChangedEvent(
            order.Id, order.BuyerId, order.SellerId,
            prev.ToString(), nameof(OrderStatus.ReadyForPickup), now), ct);
    }
}
