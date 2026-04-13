using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Orders.ConfirmOrder;

public sealed class ConfirmOrderCommandHandler(OrderDbContext db, IEventBus eventBus)
    : IRequestHandler<ConfirmOrderCommand>
{
    public async Task Handle(ConfirmOrderCommand request, CancellationToken ct)
    {
        var order = await db.Orders.FindAsync([request.OrderId], ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (order.SellerId != request.SellerId)
            throw new ForbiddenException("You can only confirm your own orders.");

        if (order.Status != OrderStatus.Pending)
            throw new ConflictException($"Order cannot be confirmed from status '{order.Status}'.");

        var now = DateTimeOffset.UtcNow;
        var prev = order.Status;
        order.Status = OrderStatus.Confirmed;
        order.UpdatedAt = now;

        db.OrderStatusHistory.Add(new OrderStatusHistory(Guid.NewGuid())
        {
            OrderId = order.Id,
            FromStatus = prev,
            ToStatus = OrderStatus.Confirmed,
            ChangedById = request.SellerId,
            ChangedAt = now
        });

        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new OrderStatusChangedEvent(
            order.Id, order.BuyerId, order.SellerId,
            prev.ToString(), nameof(OrderStatus.Confirmed), now), ct);
    }
}
