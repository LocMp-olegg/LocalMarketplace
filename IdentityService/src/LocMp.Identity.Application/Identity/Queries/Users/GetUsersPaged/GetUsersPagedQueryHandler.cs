using AutoMapper;
using LocMp.BuildingBlocks.Application.Common;
using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Queries.Users.GetUsersPaged;

public sealed class GetUsersPagedQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    : IRequestHandler<GetUsersPagedQuery, PagedResult<UserDto>>
{
    public async Task<PagedResult<UserDto>> Handle(GetUsersPagedQuery request, CancellationToken ct)
    {
        var query = userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.RegisteredAt);

        var totalCount = await query.CountAsync(ct).ConfigureAwait(false);

        var users = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var items = new List<UserDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            items.Add(mapper.Map<UserDto>(user) with { Roles = [.. roles] });
        }

        return PagedResult<UserDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}