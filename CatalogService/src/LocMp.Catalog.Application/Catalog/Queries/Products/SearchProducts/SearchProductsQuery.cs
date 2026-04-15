using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Enums;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.SearchProducts;

public sealed record SearchProductsQuery(
    string? Search,
    Guid? CategoryId,
    IReadOnlyList<string>? Tags,
    decimal? MinPrice,
    decimal? MaxPrice,
    Guid? ShopId = null,
    bool IsInStock = false,
    ProductSortBy Sort = ProductSortBy.Newest,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductSummaryDto>>, ICacheableQuery
{
    public string CacheKey =>
        $"products:search:{Search ?? "-"}:{CategoryId?.ToString() ?? "-"}:" +
        $"{(Tags is { Count: > 0 } ? string.Join(",", Tags.OrderBy(t => t)) : "-")}:" +
        $"{MinPrice?.ToString() ?? "-"}:{MaxPrice?.ToString() ?? "-"}:" +
        $"{ShopId?.ToString() ?? "-"}:{IsInStock}:{Sort}:{Page}:{PageSize}";

    public TimeSpan Ttl => TimeSpan.FromMinutes(2);
}
