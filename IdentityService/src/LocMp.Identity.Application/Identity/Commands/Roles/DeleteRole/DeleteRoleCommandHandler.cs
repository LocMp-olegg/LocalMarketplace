using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Application.Identity.Commands.Roles.DeleteRole;

public sealed class DeleteRoleCommandHandler(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager
) : IRequestHandler<DeleteRoleCommand, Unit>
{
    public async Task<Unit> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.Id.ToString()).ConfigureAwait(false);

        if (role is null)
            throw new KeyNotFoundException($"Role with id '{request.Id}' was not found.");

        var usersInRole = await userManager.GetUsersInRoleAsync(role.Name!);
        if (usersInRole.Any())
            throw new InvalidOperationException($"Cannot delete role '{role.Name}' because it is currently assigned to {usersInRole.Count} user(s).");
        
        var result = await roleManager.DeleteAsync(role).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to delete role '{role.Name}': {errors}");
        }

        return Unit.Value;
    }
}