using LocMp.Identity.Application.DTOs.Common;
using LocMp.Identity.Application.DTOs.User;
using MediatR;

namespace LocMp.Identity.Application.Identity.Queries.Users.GetUsersPaged;

public sealed record GetUsersPagedQuery(int Page, int PageSize) : IRequest<PagedResult<UserDto>>;

