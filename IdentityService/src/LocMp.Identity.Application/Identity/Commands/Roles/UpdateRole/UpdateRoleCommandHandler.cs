using AutoMapper;
using LocMp.Identity.Application.DTOs.Role;
using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Application.Identity.Commands.Roles.UpdateRole;

public sealed class UpdateRoleCommandHandler(
    RoleManager<ApplicationRole> roleManager,
    IMapper mapper
) : IRequestHandler<UpdateRoleCommand, RoleDto>
{
    public async Task<RoleDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.Id.ToString()).ConfigureAwait(false);

        if (role is null)
            throw new KeyNotFoundException($"Role with id '{request.Id}' was not found.");

        role.Name = request.Name;
        role.NormalizedName = request.Name.ToUpperInvariant();
        role.Active = request.Active;

        var result = await roleManager.UpdateAsync(role).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update role '{request.Name}': {errors}");
        }

        return mapper.Map<RoleDto>(role);
    }
}