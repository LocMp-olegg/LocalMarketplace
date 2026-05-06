using LocMp.Identity.Application.DTOs.UserAddress;
using MediatR;

namespace LocMp.Identity.Application.Identity.Queries.Addresses.GetUserAddressById;

public sealed record GetUserAddressByIdQuery(Guid UserId, Guid AddressId) : IRequest<UserAddressDto>;
