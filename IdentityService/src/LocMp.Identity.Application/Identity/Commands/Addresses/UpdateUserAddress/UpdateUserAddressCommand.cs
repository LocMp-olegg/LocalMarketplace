using LocMp.Identity.Application.DTOs.UserAddress;
using MediatR;

namespace LocMp.Identity.Application.Identity.Commands.Addresses.UpdateUserAddress;

public sealed record UpdateUserAddressCommand(
    Guid UserId,
    Guid AddressId,
    string Title,
    string City,
    string Street,
    string HouseNumber,
    string? Apartment,
    string? Entrance,
    string? Floor,
    double? Latitude,
    double? Longitude
) : IRequest<UserAddressDto>;
