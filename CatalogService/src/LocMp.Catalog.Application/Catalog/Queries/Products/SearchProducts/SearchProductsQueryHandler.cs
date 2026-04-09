using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.SearchProducts;

public sealed class SearchProductsQueryHandler(CatalogDbContext db)
    : IRequestHandler<SearchProductsQuery, PagedResult<ProductSummaryDto>>
{
    public async Task<PagedResult<ProductSummaryDto>> Handle(
        SearchProductsQuery request, CancellationToken ct)
    {
        var query = db.Products
            .Where(p => p.IsActive && !p.IsDeleted && p.StockQuantity > 0);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{request.Search}%")
                                     || EF.Functions.ILike(p.Description ?? "", $"%{request.Search}%"));

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (request.Tags is { Count: > 0 })
        {
            var normalized = request.Tags.Select(t => t.ToLowerInvariant()).ToList();
            query = query.Where(p => p.ProductTags.Any(pt => normalized.Contains(pt.Tag.Name.ToLower())));
        }

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

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
