using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Enums;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetMyProducts;

public sealed record GetMyProductsQuery(
    Guid SellerId,
    Guid? ShopId = null,
    Guid? CategoryId = null,
    string? Search = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? IsActive = null,
    bool IsInStock = false,
    ProductSortBy Sort = ProductSortBy.Newest,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductSummaryDto>>;
