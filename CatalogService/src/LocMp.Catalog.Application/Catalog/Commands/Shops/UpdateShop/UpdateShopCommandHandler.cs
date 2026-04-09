using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using NetTopologySuite.Geometries;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.UpdateShop;

public sealed class UpdateShopCommandHandler(CatalogDbContext db, IMapper mapper)
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
        shop.Location = location;
        shop.IsActive = request.IsActive;
        shop.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return mapper.Map<ShopDto>(shop);
    }
}
