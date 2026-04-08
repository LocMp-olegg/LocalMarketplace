using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Application.Identity.Commands.Users.UnblockUser;

public sealed class UnblockUserCommandHandler(
    UserManager<ApplicationUser> userManager) : IRequestHandler<UnblockUserCommand, Unit>
{
    public async Task<Unit> Handle(UnblockUserCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString())
                   ?? throw new KeyNotFoundException($"User {request.UserId} not found");

        await userManager.SetLockoutEndDateAsync(user, null);

        user.Active = true;
        await userManager.UpdateAsync(user);

        return Unit.Value;
    }
}