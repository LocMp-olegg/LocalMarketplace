using AutoMapper;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using NetTopologySuite.Geometries;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.CreateShop;

public sealed class CreateShopCommandHandler(CatalogDbContext db, IMapper mapper)
    : IRequestHandler<CreateShopCommand, ShopDto>
{
    public async Task<ShopDto> Handle(CreateShopCommand request, CancellationToken ct)
    {
        Point? location = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
            location = new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 };

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
