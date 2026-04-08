using AutoMapper;
using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LocMp.Identity.Application.Identity.Queries.Users.GetUsersByRoleId;

public sealed class GetUsersByRoleIdQueryHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IMapper mapper)
    : IRequestHandler<GetUsersByRoleIdQuery, IReadOnlyList<UserDto>>
{
    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersByRoleIdQuery request, CancellationToken cancellationToken)
    {
        var role = await roleManager.FindByIdAsync(request.RoleId.ToString()).ConfigureAwait(false);

        if (role is null)
            throw new KeyNotFoundException($"Role with id '{request.RoleId}' was not found.");

        var usersInRole = await userManager.GetUsersInRoleAsync(role.Name!)
            .ConfigureAwait(false);

        var result = mapper.Map<IReadOnlyList<UserDto>>(usersInRole);

        return result;
    }
}