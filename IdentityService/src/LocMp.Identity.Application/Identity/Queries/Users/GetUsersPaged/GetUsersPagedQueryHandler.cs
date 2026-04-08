using AutoMapper;
using AutoMapper.QueryableExtensions;
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

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<UserDto>(mapper.ConfigurationProvider)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        return PagedResult<UserDto>.Create(items, totalCount, request.Page, request.PageSize);
    }
}
