using AutoMapper;
using LocMp.Identity.Application.DTOs.UserAddress;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Identity.Application.Identity.Queries.Addresses.GetUserAddresses;

public sealed class GetUserAddressesQueryHandler(ApplicationDbContext db, IMapper mapper)
    : IRequestHandler<GetUserAddressesQuery, IReadOnlyList<UserAddressDto>>
{
    public async Task<IReadOnlyList<UserAddressDto>> Handle(GetUserAddressesQuery request, CancellationToken ct)
    {
        var addresses = await db.UserAddresses
            .AsNoTracking()
            .Where(a => a.UserId == request.UserId)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync(ct);

        return mapper.Map<IReadOnlyList<UserAddressDto>>(addresses);
    }
}
