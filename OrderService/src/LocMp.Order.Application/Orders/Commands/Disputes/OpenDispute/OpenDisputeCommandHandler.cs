using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Dispute;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Disputes.OpenDispute;

public sealed class OpenDisputeCommandHandler(OrderDbContext db, IEventBus eventBus)
    : IRequestHandler<OpenDisputeCommand>
{
    private static readonly HashSet<OrderStatus> DisputableStatuses =
    [
        OrderStatus.Confirmed,
        OrderStatus.ReadyForPickup,
        OrderStatus.InDelivery
    ];

    public async Task Handle(OpenDisputeCommand request, CancellationToken ct)
    {
        var order = await db.Orders
                        .Include(o => o.Dispute)
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (order.BuyerId != request.InitiatorId && order.SellerId != request.InitiatorId)
            throw new ForbiddenException("Only order participants can open a dispute.");

        if (!DisputableStatuses.Contains(order.Status))
            throw new ConflictException($"Cannot open dispute for order in status '{order.Status}'.");

        if (order.Dispute is not null)
            throw new ConflictException("A dispute is already open for this order.");

        var now = DateTimeOffset.UtcNow;
        var disputeId = Guid.NewGuid();

        var (_, history) = order.TransitionTo(OrderStatus.Disputed, request.InitiatorId, now, request.Reason);

        db.Disputes.Add(new Dispute(disputeId)
        {
            OrderId = order.Id,
            InitiatorId = request.InitiatorId,
            Reason = request.Reason,
            Status = DisputeStatus.Open,
            CreatedAt = now
        });
        db.OrderStatusHistory.Add(history);

        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new DisputeOpenedEvent(
            disputeId, order.Id, request.InitiatorId, now), ct);
    }
}