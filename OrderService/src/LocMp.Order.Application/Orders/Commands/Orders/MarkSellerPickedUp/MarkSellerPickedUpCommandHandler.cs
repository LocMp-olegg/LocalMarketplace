using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Orders.MarkSellerPickedUp;

public sealed class MarkSellerPickedUpCommandHandler(OrderDbContext db, IEventBus eventBus)
    : IRequestHandler<MarkSellerPickedUpCommand>
{
    public async Task Handle(MarkSellerPickedUpCommand request, CancellationToken ct)
    {
        var order = await db.Orders
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (order.SellerId != request.SellerId)
            throw new ForbiddenException("You are not the seller for this order.");

        if (!order.IsSellerDelivery)
            throw new ConflictException("This order is not set up for seller delivery.");

        if (order.Status != OrderStatus.ReadyForCourier)
            throw new ConflictException($"Order must be ReadyForCourier to start delivery (current: {order.Status}).");

        var now = DateTimeOffset.UtcNow;
        var (prev, history) = order.TransitionTo(OrderStatus.InDelivery, request.SellerId, now,
            "Seller picked up the order for delivery");
        db.OrderStatusHistory.Add(history);

        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new OrderStatusChangedEvent(
            order.Id, order.BuyerId, order.SellerId,
            prev.ToString(), nameof(OrderStatus.InDelivery), now), ct);
    }
}