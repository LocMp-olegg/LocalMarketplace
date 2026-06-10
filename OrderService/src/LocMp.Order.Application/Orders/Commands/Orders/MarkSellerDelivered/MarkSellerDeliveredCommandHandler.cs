using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Orders.MarkSellerDelivered;

public sealed class MarkSellerDeliveredCommandHandler(OrderDbContext db, IEventBus eventBus)
    : IRequestHandler<MarkSellerDeliveredCommand>
{
    public async Task Handle(MarkSellerDeliveredCommand request, CancellationToken ct)
    {
        var order = await db.Orders
                        .Include(o => o.Items)
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (order.SellerId != request.SellerId)
            throw new ForbiddenException("You are not the seller for this order.");

        if (!order.IsSellerDelivery)
            throw new ConflictException("This order is not set up for seller delivery.");

        if (order.Status != OrderStatus.InDelivery)
            throw new ConflictException($"Order must be InDelivery to mark as delivered (current: {order.Status}).");

        var now = DateTimeOffset.UtcNow;
        var prev = order.Status;

        order.Status = OrderStatus.Completed;
        order.CompletedAt = now;
        order.UpdatedAt = now;

        db.OrderStatusHistory.Add(new OrderStatusHistory(Guid.NewGuid())
        {
            OrderId = order.Id,
            FromStatus = prev,
            ToStatus = OrderStatus.Completed,
            ChangedById = request.SellerId,
            ChangedAt = now
        });

        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new OrderCompletedEvent(
            order.Id, order.BuyerId, order.SellerId, order.SellerName,
            CourierId: null,
            order.Items.Select(i =>
                    new OrderedProductItem(i.ProductId, i.ProductName, i.Quantity, i.Subtotal, i.ShopId, i.ShopName))
                .ToList(),
            order.TotalAmount,
            IsSellerDelivery: true,
            now), ct);

        await eventBus.PublishAsync(new OrderStatusChangedEvent(
            order.Id, order.BuyerId, order.SellerId,
            prev.ToString(), nameof(OrderStatus.Completed), now), ct);
    }
}