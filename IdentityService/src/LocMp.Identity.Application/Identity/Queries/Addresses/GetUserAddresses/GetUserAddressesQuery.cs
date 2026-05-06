using LocMp.Identity.Application.DTOs.UserAddress;
using MediatR;

namespace LocMp.Identity.Application.Identity.Queries.Addresses.GetUserAddresses;

public sealed record GetUserAddressesQuery(Guid UserId) : IRequest<IReadOnlyList<UserAddressDto>>;
