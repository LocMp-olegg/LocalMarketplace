using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Commands.Users.UpdateUserRoles;

public sealed class UpdateUserRolesCommandHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager
) : IRequestHandler<UpdateUserRolesCommand, Unit>
{
    public async Task<Unit> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await GetUserAsync(request.UserId);

        var targetRoleNames = PrepareTargetRoles(request.Roles);
        await ValidateRolesExistAsync(targetRoleNames, cancellationToken);

        var currentRoles = await userManager.GetRolesAsync(user);
        var rolesToAdd = targetRoleNames.Except(currentRoles).ToArray();
        var rolesToRemove = currentRoles.Except(targetRoleNames).ToArray();

        await ApplyRoleChangesAsync(user, rolesToAdd, rolesToRemove);

        return Unit.Value;
    }

    private async Task<ApplicationUser> GetUserAsync(Guid userId)
    {
        return await userManager.FindByIdAsync(userId.ToString()) 
               ?? throw new KeyNotFoundException($"User with id '{userId}' was not found.");
    }

    private static string[] PrepareTargetRoles(IEnumerable<UserRole> roles)
    {
        var roleNames = roles.Select(r => r.ToString()).Distinct().ToArray();
        
        if (roleNames.Length == 0)
            throw new InvalidOperationException("A user must have at least one role.");

        return roleNames;
    }

    private async Task ValidateRolesExistAsync(string[] roleNames, CancellationToken ct)
    {
        var existingCount = await roleManager.Roles
            .CountAsync(r => roleNames.Contains(r.Name!), ct);

        if (existingCount != roleNames.Length)
            throw new KeyNotFoundException("One or more roles do not exist in the system.");
    }

    private async Task ApplyRoleChangesAsync(ApplicationUser user, string[] toAdd, string[] toRemove)
    {
        if (toAdd.Length > 0)
        {
            var result = await userManager.AddToRolesAsync(user, toAdd);
            EnsureIdentitySuccess(result, $"Failed to add roles to user {user.Email}");
        }

        if (toRemove.Length > 0)
        {
            var result = await userManager.RemoveFromRolesAsync(user, toRemove);
            EnsureIdentitySuccess(result, $"Failed to remove roles from user {user.Email}");
        }
    }

    private static void EnsureIdentitySuccess(IdentityResult result, string errorMessage)
    {
        if (result.Succeeded) return;

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        throw new InvalidOperationException($"{errorMessage}: {errors}");
    }
}