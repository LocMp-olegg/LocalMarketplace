using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Dispute;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Disputes.ResolveDispute;

public sealed class ResolveDisputeCommandHandler(OrderDbContext db, IEventBus eventBus)
    : IRequestHandler<ResolveDisputeCommand>
{
    public async Task Handle(ResolveDisputeCommand request, CancellationToken ct)
    {
        var dispute = await db.Disputes
                          .Include(d => d.Order)
                          .FirstOrDefaultAsync(d => d.Id == request.DisputeId, ct)
                      ?? throw new NotFoundException($"Dispute '{request.DisputeId}' not found.");

        if (dispute.Status != DisputeStatus.Open)
            throw new ConflictException("Dispute is already resolved or closed.");

        var now = DateTimeOffset.UtcNow;
        dispute.Status = DisputeStatus.Resolved;
        dispute.Resolution = request.Resolution;
        dispute.ResolvedAt = now;

        var (_, history) = dispute.Order.TransitionTo(OrderStatus.Cancelled, request.AdminId, now,
            $"Dispute resolved: {request.Resolution}");

        db.OrderStatusHistory.Add(history);
        await db.SaveChangesAsync(ct);

        var minutesOpen = (int)(now - dispute.CreatedAt).TotalMinutes;
        await eventBus.PublishAsync(new DisputeResolvedEvent(
            dispute.Id, dispute.Order.Id, minutesOpen, now), ct);
    }
}