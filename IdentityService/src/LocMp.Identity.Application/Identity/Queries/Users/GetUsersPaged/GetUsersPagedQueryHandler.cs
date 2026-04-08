using AutoMapper;
using AutoMapper.QueryableExtensions;
using LocMp.Identity.Application.DTOs.Common;
using LocMp.Identity.Application.DTOs.User;
using LocMp.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Queries.Users.GetUsersPaged;

public sealed class GetUsersPagedQueryHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
    : IRequestHandler<GetUsersPagedQuery, PagedResult<UserDto>>
{
    public async Task<PagedResult<UserDto>> Handle(GetUsersPagedQuery request, CancellationToken cancellationToken)
    {
        if (request.Page <= 0)
            throw new ArgumentOutOfRangeException(nameof(request.Page), "Page must be greater than zero.");

        if (request.PageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(request.PageSize), "PageSize must be greater than zero.");

        var query = userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.RegisteredAt);

        var totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ProjectTo<UserDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedResult<UserDto>(items, totalCount, request.Page, request.PageSize);
    }
}