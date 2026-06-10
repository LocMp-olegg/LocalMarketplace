using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Identity;
using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Domain.Enums;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Commands.Courier.ResignCourier;

public sealed class ResignCourierCommandHandler(
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext db,
    IEventBus eventBus
) : IRequestHandler<ResignCourierCommand, Unit>
{
    public async Task<Unit> Handle(ResignCourierCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString()).ConfigureAwait(false)
                   ?? throw new NotFoundException($"User '{request.UserId}' not found.");

        var profile = await db.CourierProfiles
                          .FirstOrDefaultAsync(x => x.CourierId == request.UserId, ct)
                      ?? throw new NotFoundException($"Courier profile for user '{request.UserId}' not found.");

        db.CourierProfiles.Remove(profile);
        await db.SaveChangesAsync(ct);

        var roleResult = await userManager.RemoveFromRoleAsync(user, nameof(UserRole.Courier)).ConfigureAwait(false);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to remove Courier role: {errors}");
        }

        await eventBus.PublishAsync(
            new UserLostCourierStatusEvent(user.Id, DateTimeOffset.UtcNow), ct);

        return Unit.Value;
    }
}