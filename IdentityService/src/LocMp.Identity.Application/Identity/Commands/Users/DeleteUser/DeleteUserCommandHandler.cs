using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Application.Identity.Commands.Users.DeleteUser;

public sealed class DeleteUserCommandHandler(
    UserManager<ApplicationUser> userManager
) : IRequestHandler<DeleteUserCommand, Unit>
{
    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString()).ConfigureAwait(false);

        if (user is null)
            throw new KeyNotFoundException($"User with id '{request.Id}' was not found.");

        var result = await userManager.DeleteAsync(user).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to delete user '{user.Email}': {errors}");
        }

        return Unit.Value;
    }
}