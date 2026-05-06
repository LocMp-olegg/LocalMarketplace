using LocMp.Identity.Application.DTOs.UserAddress;
using MediatR;

namespace LocMp.Identity.Application.Identity.Commands.Addresses.CreateUserAddress;

public sealed record CreateUserAddressCommand(
    Guid UserId,
    string Title,
    string City,
    string Street,
    string HouseNumber,
    string? Apartment,
    string? Entrance,
    string? Floor,
    double? Latitude,
    double? Longitude,
    bool IsDefault
) : IRequest<UserAddressDto>;
