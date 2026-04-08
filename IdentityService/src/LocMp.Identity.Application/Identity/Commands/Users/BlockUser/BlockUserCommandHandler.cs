using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Identity;
using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Application.Identity.Commands.Users.BlockUser;

public sealed class BlockUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    IEventBus eventBus
) : IRequestHandler<BlockUserCommand, Unit>
{
    public async Task<Unit> Handle(BlockUserCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString())
                   ?? throw new NotFoundException($"User {request.UserId} not found");

        var blockedUntil = DateTimeOffset.UtcNow.AddMinutes(request.DurationInMinutes);

        await userManager.SetLockoutEnabledAsync(user, true);
        await userManager.SetLockoutEndDateAsync(user, blockedUntil);
        await userManager.ResetAccessFailedCountAsync(user);

        user.Active = false;
        await userManager.UpdateAsync(user);

        await eventBus.PublishAsync(
            new UserBlockedEvent(user.Id, blockedUntil, DateTimeOffset.UtcNow), ct);

        return Unit.Value;
    }
}