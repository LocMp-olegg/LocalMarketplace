using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.CreateShop;

public sealed class CreateShopCommandHandler(CatalogDbContext db, IMapper mapper)
    : IRequestHandler<CreateShopCommand, ShopDto>
{
    public async Task<ShopDto> Handle(CreateShopCommand request, CancellationToken ct)
    {
        var nameConflict = await db.Shops.AnyAsync(
            s => s.SellerId == request.SellerId
                 && s.BusinessName == request.BusinessName
                 && s.IsActive, ct);
        if (nameConflict)
            throw new ConflictException($"You already have an active shop named '{request.BusinessName}'.");

        Point? location = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
            location = new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 };

        ShopAddress? address = null;
        if (request.City is not null && request.Street is not null && request.HouseNumber is not null)
            address = new ShopAddress
            {
                City = request.City,
                Street = request.Street,
                HouseNumber = request.HouseNumber,
                Apartment = request.Apartment,
                Entrance = request.Entrance,
                Floor = request.Floor
            };

        var shop = new Shop(Guid.NewGuid())
        {
            SellerId = request.SellerId,
            BusinessName = request.BusinessName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Description = request.Description,
            Inn = request.Inn,
            BusinessType = request.BusinessType,
            WorkingHours = request.WorkingHours,
            ServiceRadiusMeters = request.ServiceRadiusMeters,
            Location = location,
            Address = address,
            IsVerified = true,
            VerifiedAt = DateTimeOffset.UtcNow,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Shops.Add(shop);
        await db.SaveChangesAsync(ct);

        return mapper.Map<ShopDto>(shop);
    }
}
