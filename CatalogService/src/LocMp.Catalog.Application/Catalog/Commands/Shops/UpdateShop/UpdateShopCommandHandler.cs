using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using StackExchange.Redis;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.UpdateShop;

public sealed class UpdateShopCommandHandler(CatalogDbContext db, IMapper mapper, IConnectionMultiplexer redis)
    : IRequestHandler<UpdateShopCommand, ShopDto>
{
    public async Task<ShopDto> Handle(UpdateShopCommand request, CancellationToken ct)
    {
        var shop = await db.Shops.FindAsync([request.ShopId], ct)
                   ?? throw new NotFoundException($"Shop '{request.ShopId}' not found.");

        if (!request.IsAdmin && shop.SellerId != request.RequesterId)
            throw new ForbiddenException("You can only update your own shops.");

        Point? location = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
            location = new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 };

        shop.BusinessName = request.BusinessName;
        shop.PhoneNumber = request.PhoneNumber;
        shop.Email = request.Email;
        shop.Description = request.Description;
        shop.Inn = request.Inn;
        shop.BusinessType = request.BusinessType;
        shop.WorkingHours = request.WorkingHours;
        shop.ServiceRadiusMeters = request.ServiceRadiusMeters;
        shop.Address = request.City is not null && request.Street is not null && request.HouseNumber is not null
            ? new ShopAddress
            {
                City = request.City,
                Street = request.Street,
                HouseNumber = request.HouseNumber,
                Apartment = request.Apartment,
                Entrance = request.Entrance,
                Floor = request.Floor
            }
            : null;
        var locationChanged = shop.Location?.X != location?.X || shop.Location?.Y != location?.Y;
        var activeStatusChanged = shop.IsActive != request.IsActive;

        shop.Location = location;
        shop.IsActive = request.IsActive;
        shop.UpdatedAt = DateTimeOffset.UtcNow;

        if (locationChanged)
        {
            await db.Products
                .Where(p => p.ShopId == shop.Id && !p.IsDeleted)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Location, location)
                    .SetProperty(p => p.UpdatedAt, DateTimeOffset.UtcNow), ct);
        }

        await db.SaveChangesAsync(ct);

        if (activeStatusChanged)
        {
            var productIds = await db.Products
                .Where(p => p.ShopId == shop.Id && !p.IsDeleted)
                .Select(p => p.Id)
                .ToListAsync(ct);

            await InvalidateCachesAsync(productIds);
        }

        return mapper.Map<ShopDto>(shop);
    }

    private async Task InvalidateCachesAsync(IReadOnlyList<Guid> productIds)
    {
        var server = redis.GetServer(redis.GetEndPoints()[0]);
        var database = redis.GetDatabase();

        var listPatterns = new[] { "locmp-catalog:products:search:*", "locmp-catalog:products:location:*" };
        foreach (var pattern in listPatterns)
        {
            var keys = server.Keys(pattern: pattern).ToArray();
            if (keys.Length > 0)
                await database.KeyDeleteAsync(keys);
        }

        if (productIds.Count > 0)
        {
            var productKeys = productIds
                .Select(id => new RedisKey($"locmp-catalog:product:{id}"))
                .ToArray();
            await database.KeyDeleteAsync(productKeys);
        }
    }
}
