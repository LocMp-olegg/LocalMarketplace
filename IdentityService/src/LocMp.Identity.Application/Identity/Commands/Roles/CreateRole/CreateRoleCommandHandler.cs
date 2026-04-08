using AutoMapper;
using LocMp.Identity.Application.DTOs.Role;
using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Application.Identity.Commands.Roles.CreateRole;

public sealed class CreateRoleCommandHandler(
    RoleManager<ApplicationRole> roleManager,
    IMapper mapper
) : IRequestHandler<CreateRoleCommand, RoleDto>
{
    public async Task<RoleDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleExists = await roleManager.RoleExistsAsync(request.Name).ConfigureAwait(false);
        if (roleExists)
            throw new InvalidOperationException($"Role '{request.Name}' already exists.");

        var role = new ApplicationRole
        {
            Name = request.Name,
            NormalizedName = request.Name.ToUpperInvariant(),
            Active = request.Active
        };

        var result = await roleManager.CreateAsync(role).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create role '{request.Name}': {errors}");
        }

        return mapper.Map<RoleDto>(role);
    }
}