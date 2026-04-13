using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Entities;
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

        if (order.Status != OrderStatus.InDelivery)
            throw new ConflictException($"Order must be InDelivery to mark as picked up (current: {order.Status}).");

        if (order.CourierAssignment.PickedUpAt.HasValue)
            throw new ConflictException("Order is already marked as picked up.");

        var now = DateTimeOffset.UtcNow;
        order.CourierAssignment.PickedUpAt = now;
        order.UpdatedAt = now;

        db.OrderStatusHistory.Add(new OrderStatusHistory(Guid.NewGuid())
        {
            OrderId = order.Id,
            FromStatus = OrderStatus.InDelivery,
            ToStatus = OrderStatus.InDelivery,
            Comment = "Courier picked up the order",
            ChangedById = request.CourierId,
            ChangedAt = now
        });

        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new OrderStatusChangedEvent(
            order.Id, order.BuyerId, order.SellerId,
            nameof(OrderStatus.InDelivery), nameof(OrderStatus.InDelivery), now), ct);
    }
}