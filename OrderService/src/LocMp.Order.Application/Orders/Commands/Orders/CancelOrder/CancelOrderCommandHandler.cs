using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Clients;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Orders.CancelOrder;

public sealed class CancelOrderCommandHandler(
    OrderDbContext db,
    CatalogServiceClient catalogClient,
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
        var prev = order.Status;
        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = now;

        db.OrderStatusHistory.Add(new OrderStatusHistory(Guid.NewGuid())
        {
            OrderId = order.Id,
            FromStatus = prev,
            ToStatus = OrderStatus.Cancelled,
            Comment = request.Comment,
            ChangedById = request.RequesterId,
            ChangedAt = now
        });

        await db.SaveChangesAsync(ct);

        // Release reserved stock (fire-and-forget on failure — stock will reconcile via events later)
        foreach (var item in order.Items)
        {
            try { await catalogClient.ReleaseStockAsync(item.ProductId, item.Quantity, order.Id, ct); }
            catch { /* log and continue */ }
        }

        await eventBus.PublishAsync(new OrderStatusChangedEvent(
            order.Id, order.BuyerId, order.SellerId,
            prev.ToString(), OrderStatus.Cancelled.ToString(), now), ct);
    }
}
