using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Orders.CompleteOrder;

public sealed class CompleteOrderCommandHandler(OrderDbContext db, IEventBus eventBus)
    : IRequestHandler<CompleteOrderCommand>
{
    private static readonly HashSet<OrderStatus> CompletableStatuses =
    [
        OrderStatus.ReadyForPickup,
        OrderStatus.InDelivery
    ];

    public async Task Handle(CompleteOrderCommand request, CancellationToken ct)
    {
        var order = await db.Orders
                        .Include(o => o.CourierAssignment)
                        .Include(o => o.Items)
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (order.BuyerId != request.BuyerId)
            throw new ForbiddenException("Only the buyer can confirm receipt.");

        if (!CompletableStatuses.Contains(order.Status))
            throw new ConflictException($"Order cannot be completed from status '{order.Status}'.");

        var now = DateTimeOffset.UtcNow;
        var (prev, history) = order.TransitionTo(OrderStatus.Completed, request.BuyerId, now);

        if (order.CourierAssignment is not null)
            order.CourierAssignment.DeliveredAt = now;

        db.OrderStatusHistory.Add(history);
        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new OrderCompletedEvent(
            order.Id, order.BuyerId, order.SellerId,
            order.CourierAssignment?.CourierId,
            order.Items.Select(i => new OrderedProductItem(i.ProductId, i.ProductName)).ToList(),
            order.TotalAmount,
            now), ct);

        await eventBus.PublishAsync(new OrderStatusChangedEvent(
            order.Id, order.BuyerId, order.SellerId,
            prev.ToString(), nameof(OrderStatus.Completed), now), ct);
    }
}