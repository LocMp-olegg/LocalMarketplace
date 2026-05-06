using AutoMapper;
using LocMp.Identity.Application.DTOs.UserAddress;
using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocMp.Identity.Application.Identity.Commands.Addresses.CreateUserAddress;

public sealed class CreateUserAddressCommandHandler(ApplicationDbContext db, IMapper mapper)
    : IRequestHandler<CreateUserAddressCommand, UserAddressDto>
{
    public async Task<UserAddressDto> Handle(CreateUserAddressCommand request, CancellationToken ct)
    {
        if (request.IsDefault)
        {
            await db.UserAddresses
                .Where(a => a.UserId == request.UserId && a.IsDefault)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), ct);
        }

        var address = new UserAddress
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Title = request.Title,
            City = request.City,
            Street = request.Street,
            HouseNumber = request.HouseNumber,
            Apartment = request.Apartment,
            Entrance = request.Entrance,
            Floor = request.Floor,
            IsDefault = request.IsDefault,
            CreatedAt = DateTimeOffset.UtcNow
        };

        if (request.Latitude.HasValue && request.Longitude.HasValue)
            address.Location = new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 };

        db.UserAddresses.Add(address);
        await db.SaveChangesAsync(ct);

        return mapper.Map<UserAddressDto>(address);
    }
}
