using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Identity;
using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Identity.Application.Identity.Commands.Users.UnblockUser;

public sealed class UnblockUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    IEventBus eventBus,
    IDistributedCache cache
) : IRequestHandler<UnblockUserCommand, Unit>
{
    public async Task<Unit> Handle(UnblockUserCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString())
            ?? throw new NotFoundException($"User {request.UserId} not found");

        await userManager.SetLockoutEndDateAsync(user, null);

        user.Active = true;
        await userManager.UpdateAsync(user);

        await cache.RemoveAsync($"locmp:blocked:{user.Id}", ct);

        await eventBus.PublishAsync(
            new UserUnblockedEvent(user.Id, DateTimeOffset.UtcNow), ct);

        return Unit.Value;
    }
}
