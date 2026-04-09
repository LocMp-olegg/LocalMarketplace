using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
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
        var role = await roleManager.FindByIdAsync(request.RoleId.ToString()).ConfigureAwait(false)
                   ?? throw new NotFoundException($"Role with id '{request.RoleId}' was not found.");

        var usersInRole = await userManager.GetUsersInRoleAsync(role.Name!).ConfigureAwait(false);

        var result = new List<UserDto>(usersInRole.Count);
        foreach (var user in usersInRole)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(mapper.Map<UserDto>(user) with { Roles = [.. roles] });
        }

        return result;
    }
}