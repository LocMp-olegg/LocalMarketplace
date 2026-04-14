using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Orders.AssignCourier;

public sealed class AssignCourierCommandHandler(OrderDbContext db, IEventBus eventBus)
    : IRequestHandler<AssignCourierCommand>
{
    public async Task Handle(AssignCourierCommand request, CancellationToken ct)
    {
        var order = await db.Orders
                        .Include(o => o.CourierAssignment)
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (order.Status != OrderStatus.Confirmed || order.DeliveryType != DeliveryType.NeighborCourier)
            throw new ConflictException("Order must be Confirmed with NeighborCourier delivery type.");

        if (order.CourierAssignment is not null)
            throw new ConflictException("Courier is already assigned to this order.");

        var now = DateTimeOffset.UtcNow;
        var (prev, history) = order.TransitionTo(OrderStatus.InDelivery, request.CourierId, now);

        db.CourierAssignments.Add(new CourierAssignment(Guid.NewGuid())
        {
            OrderId = order.Id,
            CourierId = request.CourierId,
            CourierName = request.CourierName,
            CourierPhone = request.CourierPhone,
            AssignedAt = now
        });
        db.OrderStatusHistory.Add(history);

        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new OrderStatusChangedEvent(
            order.Id, order.BuyerId, order.SellerId,
            prev.ToString(), nameof(OrderStatus.InDelivery), now), ct);
    }
}