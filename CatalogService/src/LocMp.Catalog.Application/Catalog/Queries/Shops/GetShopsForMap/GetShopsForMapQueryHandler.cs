using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocMp.Catalog.Application.Catalog.Queries.Shops.GetShopsForMap;

public sealed class GetShopsForMapQueryHandler(CatalogDbContext db)
    : IRequestHandler<GetShopsForMapQuery, IReadOnlyList<ShopMapDto>>
{
    private const int MaxResults = 300;

    public async Task<IReadOnlyList<ShopMapDto>> Handle(
        GetShopsForMapQuery request, CancellationToken ct)
    {
        var swLat = Math.Round(request.SwLat, 2);
        var swLon = Math.Round(request.SwLon, 2);
        var neLat = Math.Round(request.NeLat, 2);
        var neLon = Math.Round(request.NeLon, 2);

        var centerLat = (swLat + neLat) / 2.0;
        var centerLon = (swLon + neLon) / 2.0;
        var center = new Point(centerLon, centerLat) { SRID = 4326 };

        var latMeters = (neLat - swLat) * 111_000;
        var lonMeters = (neLon - swLon) * 111_000 * Math.Cos(centerLat * Math.PI / 180.0);
        var radiusMeters = Math.Sqrt(latMeters * latMeters + lonMeters * lonMeters) / 2.0 * 1.1;

        var raw = await db.Shops
            .Where(s =>
                s.IsActive &&
                s.Location != null &&
                s.Location.IsWithinDistance(center, radiusMeters))
            .OrderByDescending(s => s.AverageRating)
            .Take(MaxResults)
            .Select(s => new
            {
                s.Id,
                s.BusinessName,
                s.Location,
                s.AvatarUrl,
                s.AverageRating,
                s.ReviewCount,
                s.ServiceRadiusMeters,
                s.IsActive
            })
            .ToListAsync(ct);

        return raw
            .Where(s =>
                s.Location!.X >= swLon && s.Location.X <= neLon &&
                s.Location.Y >= swLat && s.Location.Y <= neLat)
            .Select(s => new ShopMapDto(
                s.Id,
                s.BusinessName,
                s.Location!.Y,
                s.Location.X,
                s.AvatarUrl,
                s.AverageRating,
                s.ReviewCount,
                s.ServiceRadiusMeters / 1000.0,
                s.IsActive))
            .ToList();
    }
}