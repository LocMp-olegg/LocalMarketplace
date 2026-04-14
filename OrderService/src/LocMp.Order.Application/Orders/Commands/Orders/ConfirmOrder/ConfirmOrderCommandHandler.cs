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

        var now = DateTimeOffset.UtcNow;
        OrderStatus prev;
        OrderStatusHistory history;
        try
        {
            (prev, history) = order.TransitionTo(OrderStatus.Confirmed, request.SellerId, now);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        db.OrderStatusHistory.Add(history);
        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new OrderStatusChangedEvent(
            order.Id, order.BuyerId, order.SellerId,
            prev.ToString(), nameof(OrderStatus.Confirmed), now), ct);
    }
}