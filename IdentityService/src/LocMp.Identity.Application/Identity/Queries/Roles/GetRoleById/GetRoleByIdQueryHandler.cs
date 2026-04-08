using AutoMapper;
using LocMp.Identity.Application.DTOs.Role;
using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Application.Identity.Queries.Roles.GetRoleById;

public sealed class GetRoleByIdQueryHandler(RoleManager<ApplicationRole> roleManager, IMapper mapper)
    : IRequestHandler<GetRoleByIdQuery, RoleDto>
{
    public async Task<RoleDto> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.Id.ToString()).ConfigureAwait(false);

        if (role is null)
            throw new KeyNotFoundException($"Role with id '{request.Id}' was not found.");

        return mapper.Map<RoleDto>(role);
    }
}