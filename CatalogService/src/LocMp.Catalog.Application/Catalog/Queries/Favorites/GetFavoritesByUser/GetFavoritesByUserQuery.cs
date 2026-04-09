using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Favorites.GetFavoritesByUser;

public sealed record GetFavoritesByUserQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<FavoriteDto>>;
