using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Dispute;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Interfaces;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Disputes.ResolveDispute;

public sealed class ResolveDisputeCommandHandler(
    OrderDbContext db,
    ICatalogClient catalogClient,
    IEventBus eventBus)
    : IRequestHandler<ResolveDisputeCommand>
{
    public async Task Handle(ResolveDisputeCommand request, CancellationToken ct)
    {
        var dispute = await db.Disputes
                          .Include(d => d.Order)
                          .ThenInclude(o => o.Items)
                          .FirstOrDefaultAsync(d => d.Id == request.DisputeId, ct)
                      ?? throw new NotFoundException($"Dispute '{request.DisputeId}' not found.");

        if (dispute.Status != DisputeStatus.Open)
            throw new ConflictException("Dispute is already resolved or closed.");

        var now = DateTimeOffset.UtcNow;
        var targetStatus = request.Outcome == DisputeOutcome.BuyerFavored
            ? OrderStatus.Cancelled
            : OrderStatus.Completed;

        dispute.Status = DisputeStatus.Resolved;
        dispute.Outcome = request.Outcome;
        dispute.Resolution = request.Resolution;
        dispute.ResolvedAt = now;

        var (_, history) = dispute.Order.TransitionTo(targetStatus, request.AdminId, now,
            $"Dispute resolved ({request.Outcome}): {request.Resolution}");

        db.OrderStatusHistory.Add(history);
        await db.SaveChangesAsync(ct);

        if (request.Outcome == DisputeOutcome.BuyerFavored)
        {
            foreach (var item in dispute.Order.Items)
            {
                try
                {
                    await catalogClient.ReleaseStockAsync(item.ProductId, item.Quantity, dispute.Order.Id, ct);
                }
                catch
                {
                    /* stock will reconcile via events */
                }
            }
        }

        var minutesOpen = (int)(now - dispute.CreatedAt).TotalMinutes;
        await eventBus.PublishAsync(new DisputeResolvedEvent(
            dispute.Id, dispute.Order.Id, minutesOpen, now), ct);
    }
}