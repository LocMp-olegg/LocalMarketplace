using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetProductsBySeller;

public sealed class GetProductsBySellerQueryHandler(CatalogDbContext db)
    : IRequestHandler<GetProductsBySellerQuery, PagedResult<ProductSummaryDto>>
{
    public async Task<PagedResult<ProductSummaryDto>> Handle(
        GetProductsBySellerQuery request, CancellationToken ct)
    {
        var query = db.Products
            .Where(p => p.SellerId == request.SellerId && !p.IsDeleted);

        if (!request.IncludeInactive)
            query = query.Where(p => p.IsActive);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductSummaryDto(
                p.Id,
                p.SellerId,
                p.ShopId,
                p.CategoryId,
                p.Name,
                p.Price,
                p.Unit,
                p.StockQuantity,
                p.IsActive,
                p.Location != null ? p.Location.Y : (double?)null,
                p.Location != null ? p.Location.X : (double?)null,
                p.Photos.Where(ph => ph.IsMain).Select(ph => ph.StorageUrl).FirstOrDefault()
                    ?? p.Photos.OrderBy(ph => ph.SortOrder).Select(ph => ph.StorageUrl).FirstOrDefault(),
                null,
                p.ProductTags.Select(pt => pt.Tag.Name).ToList()
            ))
            .ToListAsync(ct);

        return new PagedResult<ProductSummaryDto>(items, total, request.Page, request.PageSize);
    }
}
