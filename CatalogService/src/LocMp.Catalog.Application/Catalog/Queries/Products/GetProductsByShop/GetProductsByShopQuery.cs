using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Enums;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetProductsByShop;

public sealed record GetProductsByShopQuery(
    Guid ShopId,
    Guid? CategoryId = null,
    string? Search = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool IsInStock = false,
    ProductSortBy Sort = ProductSortBy.Newest,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductSummaryDto>>;
