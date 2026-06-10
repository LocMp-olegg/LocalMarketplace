using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Orders.MarkReadyForCourier;

public sealed class MarkReadyForCourierCommandHandler(OrderDbContext db, IEventBus eventBus)
    : IRequestHandler<MarkReadyForCourierCommand>
{
    public async Task Handle(MarkReadyForCourierCommand request, CancellationToken ct)
    {
        var order = await db.Orders
                        .Include(o => o.CourierAssignment)
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (order.SellerId != request.SellerId)
            throw new ForbiddenException("You can only update your own orders.");

        if (order.DeliveryType != DeliveryType.Delivery)
            throw new ConflictException("This transition is only valid for courier delivery orders.");

        if (order.Status != OrderStatus.Confirmed)
            throw new ConflictException(
                $"Order must be Confirmed to mark as ready for courier (current: {order.Status}).");

        if (order.CourierAssignment is null)
            throw new ConflictException("A courier must be assigned before marking the order as ready.");

        var now = DateTimeOffset.UtcNow;
        var (prev, history) = order.TransitionTo(OrderStatus.ReadyForCourier, request.SellerId, now,
            "Order is packed and ready for courier pickup");

        db.OrderStatusHistory.Add(history);
        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new OrderStatusChangedEvent(
            order.Id, order.BuyerId, order.SellerId,
            prev.ToString(), nameof(OrderStatus.ReadyForCourier), now), ct);
    }
}