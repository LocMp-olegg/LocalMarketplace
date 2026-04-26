using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetProductsByTags;

public sealed class GetProductsByTagsQueryHandler(CatalogDbContext db)
    : IRequestHandler<GetProductsByTagsQuery, PagedResult<ProductSummaryDto>>
{
    public async Task<PagedResult<ProductSummaryDto>> Handle(
        GetProductsByTagsQuery request, CancellationToken ct)
    {
        var normalizedTags = request.Tags.Select(t => t.ToLowerInvariant()).ToList();

        var query = db.Products
            .Where(p => p.IsActive && !p.IsDeleted && (p.StockQuantity > 0 || p.IsMadeToOrder))
            .Where(p => p.ProductTags.Any(pt => normalizedTags.Contains(pt.Tag.Name.ToLower())));

        var total = await query.CountAsync(ct);

        var raw = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new
            {
                p.Id,
                p.SellerId,
                p.ShopId,
                p.CategoryId,
                p.Name,
                p.Price,
                p.Unit,
                p.StockQuantity,
                p.IsActive,
                p.Location,
                MainPhotoUrl = p.Photos
                                   .Where(ph => ph.IsMain)
                                   .Select(ph => ph.StorageUrl)
                                   .FirstOrDefault()
                               ?? p.Photos
                                   .OrderBy(ph => ph.SortOrder)
                                   .Select(ph => ph.StorageUrl)
                                   .FirstOrDefault(),
                PhotoUrls = p.Photos
                    .OrderBy(ph => ph.SortOrder)
                    .Select(ph => ph.StorageUrl)
                    .ToList(),
                ShopName = p.Shop.BusinessName,
                Tags = p.ProductTags
                    .Select(pt => pt.Tag.Name)
                    .ToList(),
                p.IsMadeToOrder,
                p.LeadTimeDays,
                p.AverageRating,
                p.ReviewCount
            })
            .ToListAsync(ct);

        var items = raw.Select(p => new ProductSummaryDto(
            p.Id,
            p.SellerId,
            p.ShopId,
            p.CategoryId,
            p.Name,
            p.Price,
            p.Unit,
            p.StockQuantity,
            p.IsActive,
            p.Location?.Y,
            p.Location?.X,
            p.MainPhotoUrl,
            null,
            p.Tags,
            p.IsMadeToOrder,
            p.LeadTimeDays,
            p.PhotoUrls,
            p.ShopName,
            p.AverageRating,
            p.ReviewCount
        )).ToList();

        return new PagedResult<ProductSummaryDto>(items, total, request.Page, request.PageSize);
    }
}