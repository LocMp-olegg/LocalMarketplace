using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Shops.GetShopsForMap;

public sealed record GetShopsForMapQuery(
    double SwLat,
    double SwLon,
    double NeLat,
    double NeLon
) : IRequest<IReadOnlyList<ShopMapDto>>, ICacheableQuery
{
    public string CacheKey =>
        $"shops:map:{Math.Round(SwLat, 2):F2}:{Math.Round(SwLon, 2):F2}:" +
        $"{Math.Round(NeLat, 2):F2}:{Math.Round(NeLon, 2):F2}";

    public TimeSpan Ttl => TimeSpan.FromMinutes(5);
}