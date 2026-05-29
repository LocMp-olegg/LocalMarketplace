using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Orders.ApproveCourierApplication;

public sealed class ApproveCourierApplicationCommandHandler(OrderDbContext db, IEventBus eventBus)
    : IRequestHandler<ApproveCourierApplicationCommand>
{
    public async Task Handle(ApproveCourierApplicationCommand request, CancellationToken ct)
    {
        var application = await db.CourierApplications
                              .Include(a => a.Order)
                              .ThenInclude(o => o.CourierAssignment)
                              .Include(a => a.Order)
                              .ThenInclude(o => o.CourierApplications)
                              .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, ct)
                          ?? throw new NotFoundException($"Application '{request.ApplicationId}' not found.");

        if (application.Order.SellerId != request.SellerId)
            throw new ForbiddenException("You are not the seller for this order.");

        if (application.Status != CourierApplicationStatus.Pending)
            throw new ConflictException($"Application status is '{application.Status}', expected Pending.");

        if (application.Order.Status != OrderStatus.Confirmed)
            throw new ConflictException("Order must be Confirmed to approve a courier application.");

        if (application.Order.CourierAssignment is not null)
            throw new ConflictException("A courier is already assigned to this order.");

        var now = DateTimeOffset.UtcNow;

        application.Status = CourierApplicationStatus.Approved;
        application.UpdatedAt = now;

        db.CourierAssignments.Add(new CourierAssignment(Guid.NewGuid())
        {
            OrderId = application.OrderId,
            CourierId = application.CourierId,
            CourierName = application.CourierName,
            CourierPhone = application.CourierPhone,
            AssignedAt = now
        });

        var rejectedIds = new List<Guid>();
        foreach (var other in application.Order.CourierApplications
                     .Where(a => a.Id != application.Id && a.Status == CourierApplicationStatus.Pending))
        {
            other.Status = CourierApplicationStatus.Rejected;
            other.UpdatedAt = now;
            rejectedIds.Add(other.CourierId);
        }

        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new CourierApplicationApprovedEvent(
            application.Id, application.OrderId, application.CourierId, request.SellerId, now), ct);

        foreach (var rejectedCourierId in rejectedIds)
            await eventBus.PublishAsync(new CourierApplicationRejectedEvent(
                application.Id, application.OrderId, rejectedCourierId, now), ct);
    }
}