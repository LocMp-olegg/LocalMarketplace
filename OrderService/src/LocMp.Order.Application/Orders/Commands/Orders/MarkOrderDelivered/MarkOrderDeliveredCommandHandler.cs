using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Orders.MarkOrderDelivered;

public sealed class MarkOrderDeliveredCommandHandler(OrderDbContext db, IEventBus eventBus)
    : IRequestHandler<MarkOrderDeliveredCommand>
{
    public async Task Handle(MarkOrderDeliveredCommand request, CancellationToken ct)
    {
        var order = await db.Orders
            .Include(o => o.CourierAssignment)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
            ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (order.CourierAssignment is null || order.CourierAssignment.CourierId != request.CourierId)
            throw new ForbiddenException("You are not the assigned courier for this order.");

        if (order.Status != OrderStatus.InDelivery)
            throw new ConflictException($"Order must be InDelivery to mark as delivered (current: {order.Status}).");

        var now = DateTimeOffset.UtcNow;
        var prev = order.Status;

        order.Status = OrderStatus.Completed;
        order.CompletedAt = now;
        order.UpdatedAt = now;
        order.CourierAssignment.DeliveredAt = now;

        db.OrderStatusHistory.Add(new OrderStatusHistory(Guid.NewGuid())
        {
            OrderId = order.Id,
            FromStatus = prev,
            ToStatus = OrderStatus.Completed,
            Comment = "Delivered by courier",
            ChangedById = request.CourierId,
            ChangedAt = now
        });

        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new OrderCompletedEvent(
            order.Id, order.BuyerId, order.SellerId, request.CourierId, now), ct);

        await eventBus.PublishAsync(new OrderStatusChangedEvent(
            order.Id, order.BuyerId, order.SellerId,
            prev.ToString(), OrderStatus.Completed.ToString(), now), ct);
    }
}
