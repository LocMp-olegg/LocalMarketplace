using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetProductsByLocation;

public sealed record GetProductsByLocationQuery(
    double Latitude,
    double Longitude,
    double RadiusKm,
    Guid? CategoryId = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductSummaryDto>>, ICacheableQuery
{
    public string CacheKey =>
        $"products:location:{Latitude:F4}:{Longitude:F4}:{RadiusKm}:{CategoryId}:{Search}:{Page}:{PageSize}";
    public TimeSpan Ttl => TimeSpan.FromMinutes(2);
}
