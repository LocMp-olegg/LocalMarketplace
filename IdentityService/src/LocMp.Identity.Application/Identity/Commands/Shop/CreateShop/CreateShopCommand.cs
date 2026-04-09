using LocMp.Identity.Application.DTOs.Shop;
using LocMp.Identity.Domain.Enums;
using MediatR;

namespace LocMp.Identity.Application.Identity.Commands.Shop.CreateShop;

public sealed record CreateShopCommand(
    Guid UserId,
    string BusinessName,
    string PhoneNumber,
    string Email,
    string? Description,
    string? Inn,
    BusinessType BusinessType,
    string? WorkingHours,
    int? ServiceRadiusMeters,
    double? Latitude,
    double? Longitude
) : IRequest<ShopProfileDto>;
