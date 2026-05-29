using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Orders.RejectCourierApplication;

public sealed class RejectCourierApplicationCommandHandler(OrderDbContext db, IEventBus eventBus)
    : IRequestHandler<RejectCourierApplicationCommand>
{
    public async Task Handle(RejectCourierApplicationCommand request, CancellationToken ct)
    {
        var application = await db.CourierApplications
                              .Include(a => a.Order)
                              .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, ct)
                          ?? throw new NotFoundException($"Application '{request.ApplicationId}' not found.");

        if (application.Order.SellerId != request.SellerId)
            throw new ForbiddenException("You are not the seller for this order.");

        if (application.Status != CourierApplicationStatus.Pending)
            throw new ConflictException($"Application status is '{application.Status}', expected Pending.");

        var now = DateTimeOffset.UtcNow;
        application.Status = CourierApplicationStatus.Rejected;
        application.UpdatedAt = now;

        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new CourierApplicationRejectedEvent(
            application.Id, application.OrderId, application.CourierId, now), ct);
    }
}