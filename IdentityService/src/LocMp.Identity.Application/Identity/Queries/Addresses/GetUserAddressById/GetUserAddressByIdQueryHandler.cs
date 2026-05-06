using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Identity.Application.DTOs.UserAddress;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Queries.Addresses.GetUserAddressById;

public sealed class GetUserAddressByIdQueryHandler(ApplicationDbContext db, IMapper mapper)
    : IRequestHandler<GetUserAddressByIdQuery, UserAddressDto>
{
    public async Task<UserAddressDto> Handle(GetUserAddressByIdQuery request, CancellationToken ct)
    {
        var address = await db.UserAddresses
                          .AsNoTracking()
                          .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == request.UserId, ct)
                      ?? throw new NotFoundException($"Address '{request.AddressId}' not found.");

        return mapper.Map<UserAddressDto>(address);
    }
}
