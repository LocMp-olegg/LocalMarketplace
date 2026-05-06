using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Identity.Application.DTOs.UserAddress;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocMp.Identity.Application.Identity.Commands.Addresses.UpdateUserAddress;

public sealed class UpdateUserAddressCommandHandler(ApplicationDbContext db, IMapper mapper)
    : IRequestHandler<UpdateUserAddressCommand, UserAddressDto>
{
    public async Task<UserAddressDto> Handle(UpdateUserAddressCommand request, CancellationToken ct)
    {
        var address = await db.UserAddresses
                          .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == request.UserId, ct)
                      ?? throw new NotFoundException($"Address '{request.AddressId}' not found.");

        address.Title = request.Title;
        address.City = request.City;
        address.Street = request.Street;
        address.HouseNumber = request.HouseNumber;
        address.Apartment = request.Apartment;
        address.Entrance = request.Entrance;
        address.Floor = request.Floor;
        address.Location = request.Latitude.HasValue && request.Longitude.HasValue
            ? new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 }
            : null;

        await db.SaveChangesAsync(ct);

        return mapper.Map<UserAddressDto>(address);
    }
}
