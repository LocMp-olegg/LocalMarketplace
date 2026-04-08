using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Application.Identity.Commands.Users.BlockUser;

public sealed class BlockUserCommandHandler(
    UserManager<ApplicationUser> userManager) : IRequestHandler<BlockUserCommand, Unit>
{
    public async Task<Unit> Handle(BlockUserCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString())
                   ?? throw new KeyNotFoundException($"User {request.UserId} not found");

        var lockoutEndTime = DateTimeOffset.UtcNow.AddMinutes(request.DurationInMinutes);

        await userManager.SetLockoutEnabledAsync(user, true);
        await userManager.SetLockoutEndDateAsync(user, lockoutEndTime);

        await userManager.ResetAccessFailedCountAsync(user);

        user.Active = false;
        await userManager.UpdateAsync(user);

        return Unit.Value;
    }
}