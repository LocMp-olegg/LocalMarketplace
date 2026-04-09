using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Queries.Favorites.GetFavoritesByUser;

public sealed class GetFavoritesByUserQueryHandler(CatalogDbContext db)
    : IRequestHandler<GetFavoritesByUserQuery, PagedResult<FavoriteDto>>
{
    public async Task<PagedResult<FavoriteDto>> Handle(
        GetFavoritesByUserQuery request, CancellationToken ct)
    {
        var query = db.Favorites
            .Include(f => f.Product)
            .ThenInclude(p => p.Photos)
            .Where(f => f.UserId == request.UserId && !f.Product.IsDeleted);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(f => new FavoriteDto(
                f.Id,
                f.UserId,
                f.ProductId,
                f.Product.Name,
                f.Product.Price,
                f.Product.Photos.Where(ph => ph.IsMain).Select(ph => ph.StorageUrl).FirstOrDefault()
                    ?? f.Product.Photos.OrderBy(ph => ph.SortOrder).Select(ph => ph.StorageUrl).FirstOrDefault(),
                f.CreatedAt
            ))
            .ToListAsync(ct);

        return new PagedResult<FavoriteDto>(items, total, request.Page, request.PageSize);
    }
}
