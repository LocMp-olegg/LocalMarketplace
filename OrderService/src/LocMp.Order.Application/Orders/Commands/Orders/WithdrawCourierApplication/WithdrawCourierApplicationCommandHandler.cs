using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Orders.WithdrawCourierApplication;

public sealed class WithdrawCourierApplicationCommandHandler(OrderDbContext db)
    : IRequestHandler<WithdrawCourierApplicationCommand>
{
    public async Task Handle(WithdrawCourierApplicationCommand request, CancellationToken ct)
    {
        var application = await db.CourierApplications
                              .FirstOrDefaultAsync(a => a.Id == request.ApplicationId, ct)
                          ?? throw new NotFoundException($"Application '{request.ApplicationId}' not found.");

        if (application.CourierId != request.CourierId)
            throw new ForbiddenException("You are not the owner of this application.");

        if (application.Status != CourierApplicationStatus.Pending)
            throw new ConflictException($"Cannot withdraw an application with status '{application.Status}'.");

        application.Status = CourierApplicationStatus.Withdrawn;
        application.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}