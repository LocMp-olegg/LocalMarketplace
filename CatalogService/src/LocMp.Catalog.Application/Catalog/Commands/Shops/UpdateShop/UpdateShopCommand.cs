using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Enums;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.UpdateShop;

public sealed record UpdateShopCommand(
    Guid ShopId,
    Guid RequesterId,
    bool IsAdmin,
    string BusinessName,
    string PhoneNumber,
    string Email,
    string? Description,
    string? Inn,
    BusinessType BusinessType,
    string? WorkingHours,
    int? ServiceRadiusMeters,
    double? Latitude,
    double? Longitude,
    bool IsActive
) : IRequest<ShopDto>;
