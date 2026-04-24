using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Enums;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetMyProducts;

public sealed class GetMyProductsQueryHandler(CatalogDbContext db)
    : IRequestHandler<GetMyProductsQuery, PagedResult<ProductSummaryDto>>
{
    public async Task<PagedResult<ProductSummaryDto>> Handle(
        GetMyProductsQuery request, CancellationToken ct)
    {
        var query = db.Products
            .Where(p => p.SellerId == request.SellerId && !p.IsDeleted);

        if (request.IsActive.HasValue)
            query = query.Where(p => p.IsActive == request.IsActive.Value);

        if (request.ShopId.HasValue)
            query = query.Where(p => p.ShopId == request.ShopId.Value);

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{request.Search}%")
                                     || EF.Functions.ILike(p.Description ?? "", $"%{request.Search}%"));

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        if (request.IsInStock)
            query = query.Where(p => p.StockQuantity > 0);

        var total = await query.CountAsync(ct);

        query = request.Sort switch
        {
            ProductSortBy.PriceAsc  => query.OrderBy(p => p.Price),
            ProductSortBy.PriceDesc => query.OrderByDescending(p => p.Price),
            ProductSortBy.NameAsc   => query.OrderBy(p => p.Name),
            _                       => query.OrderByDescending(p => p.CreatedAt)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductSummaryDto(
                p.Id, p.SellerId, p.ShopId, p.CategoryId, p.Name, p.Price, p.Unit,
                p.StockQuantity, p.IsActive,
                p.Location != null ? p.Location.Y : null,
                p.Location != null ? p.Location.X : null,
                p.Photos.Where(ph => ph.IsMain).Select(ph => ph.StorageUrl).FirstOrDefault()
                    ?? p.Photos.OrderBy(ph => ph.SortOrder).Select(ph => ph.StorageUrl).FirstOrDefault(),
                null,
                p.ProductTags.Select(pt => pt.Tag.Name).ToList(),
                p.IsMadeToOrder,
                p.LeadTimeDays,
                p.Photos.OrderBy(ph => ph.SortOrder).Select(ph => ph.StorageUrl).ToList(),
                p.Shop.BusinessName,
                p.AverageRating,
                p.ReviewCount
            ))
            .ToListAsync(ct);

        return new PagedResult<ProductSummaryDto>(items, total, request.Page, request.PageSize);
    }
}
