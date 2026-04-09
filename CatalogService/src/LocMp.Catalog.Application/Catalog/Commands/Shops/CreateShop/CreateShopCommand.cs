using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Enums;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.CreateShop;

public sealed record CreateShopCommand(
    Guid SellerId,
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
) : IRequest<ShopDto>;
