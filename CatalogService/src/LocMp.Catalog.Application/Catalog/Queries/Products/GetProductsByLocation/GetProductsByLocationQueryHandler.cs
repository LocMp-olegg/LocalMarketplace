using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetProductsByLocation;

public sealed class GetProductsByLocationQueryHandler(CatalogDbContext db)
    : IRequestHandler<GetProductsByLocationQuery, PagedResult<ProductSummaryDto>>
{
    public async Task<PagedResult<ProductSummaryDto>> Handle(
        GetProductsByLocationQuery request, CancellationToken ct)
    {
        var center = new Point(request.Longitude, request.Latitude) { SRID = 4326 };
        var radiusMeters = request.RadiusKm * 1000;

        var query = db.Products
            .Where(p => p.IsActive && !p.IsDeleted && (p.StockQuantity > 0 || p.IsMadeToOrder));

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{request.Search}%"));

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        if (request.IsInStock)
            query = query.Where(p => p.StockQuantity > 0);

        query = query.Where(p => p.Location != null && p.Location.IsWithinDistance(center, radiusMeters));

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Location!.Distance(center))
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
                p.Location != null ? p.Location.Y : null,
                p.Location != null ? p.Location.X : null,
                p.Photos.Where(ph => ph.IsMain).Select(ph => ph.StorageUrl).FirstOrDefault()
                    ?? p.Photos.OrderBy(ph => ph.SortOrder).Select(ph => ph.StorageUrl).FirstOrDefault(),
                p.Location != null ? p.Location.Distance(center) : null,
                p.ProductTags.Select(pt => pt.Tag.Name).ToList(),
                p.IsMadeToOrder,
                p.LeadTimeDays
            ))
            .ToListAsync(ct);

        return new PagedResult<ProductSummaryDto>(items, total, request.Page, request.PageSize);
    }
}
