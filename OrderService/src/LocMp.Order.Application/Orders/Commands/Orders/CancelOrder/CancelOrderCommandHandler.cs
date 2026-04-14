using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Infrastructure.Interfaces;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Orders.CancelOrder;

public sealed class CancelOrderCommandHandler(
    OrderDbContext db,
    ICatalogClient catalogClient,
    IEventBus eventBus)
    : IRequestHandler<CancelOrderCommand>
{
    private static readonly HashSet<OrderStatus> CancellableStatuses =
    [
        OrderStatus.Pending,
        OrderStatus.Confirmed
    ];

    public async Task Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var order = await db.Orders
                        .Include(o => o.Items)
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (!request.IsAdmin && order.BuyerId != request.RequesterId && order.SellerId != request.RequesterId)
            throw new ForbiddenException("You are not a participant in this order.");

        if (!CancellableStatuses.Contains(order.Status))
            throw new ConflictException($"Order cannot be cancelled from status '{order.Status}'.");

        var now = DateTimeOffset.UtcNow;
        var (prev, history) = order.TransitionTo(OrderStatus.Cancelled, request.RequesterId, now, request.Comment);

        db.OrderStatusHistory.Add(history);
        await db.SaveChangesAsync(ct);

        foreach (var item in order.Items)
        {
            try
            {
                await catalogClient.ReleaseStockAsync(item.ProductId, item.Quantity, order.Id, ct);
            }
            catch
            {
                /* stock will reconcile via events */
            }
        }

        await eventBus.PublishAsync(new OrderStatusChangedEvent(
            order.Id, order.BuyerId, order.SellerId,
            prev.ToString(), nameof(OrderStatus.Cancelled), now), ct);
    }
}