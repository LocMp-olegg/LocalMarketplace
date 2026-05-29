using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Orders.MarkOrderPickedUp;

public sealed class MarkOrderPickedUpCommandHandler(OrderDbContext db, IEventBus eventBus)
    : IRequestHandler<MarkOrderPickedUpCommand>
{
    public async Task Handle(MarkOrderPickedUpCommand request, CancellationToken ct)
    {
        var order = await db.Orders
                        .Include(o => o.CourierAssignment)
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (order.CourierAssignment is null || order.CourierAssignment.CourierId != request.CourierId)
            throw new ForbiddenException("You are not the assigned courier for this order.");

        if (order.Status != OrderStatus.ReadyForCourier)
            throw new ConflictException($"Order must be ReadyForCourier to mark as picked up (current: {order.Status}).");

        if (order.CourierAssignment.PickedUpAt.HasValue)
            throw new ConflictException("Order is already marked as picked up.");

        var now = DateTimeOffset.UtcNow;
        order.CourierAssignment.PickedUpAt = now;

        var (prev, history) = order.TransitionTo(OrderStatus.InDelivery, request.CourierId, now,
            "Courier picked up the order");
        db.OrderStatusHistory.Add(history);

        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new OrderStatusChangedEvent(
            order.Id, order.BuyerId, order.SellerId,
            prev.ToString(), nameof(OrderStatus.InDelivery), now), ct);
    }
}
