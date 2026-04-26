using LocMp.BuildingBlocks.Application.Common;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Enums;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetProductsByShop;

public sealed class GetProductsByShopQueryHandler(CatalogDbContext db)
    : IRequestHandler<GetProductsByShopQuery, PagedResult<ProductSummaryDto>>
{
    public async Task<PagedResult<ProductSummaryDto>> Handle(
        GetProductsByShopQuery request, CancellationToken ct)
    {
        var shopExists = await db.Shops.AnyAsync(s => s.Id == request.ShopId && s.IsActive, ct);
        if (!shopExists)
            throw new NotFoundException($"Shop '{request.ShopId}' not found.");

        var query = db.Products
            .Where(p => p.ShopId == request.ShopId && p.IsActive && !p.IsDeleted);

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
            ProductSortBy.PriceAsc => query.OrderBy(p => p.Price),
            ProductSortBy.PriceDesc => query.OrderByDescending(p => p.Price),
            ProductSortBy.NameAsc => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var raw = await query
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