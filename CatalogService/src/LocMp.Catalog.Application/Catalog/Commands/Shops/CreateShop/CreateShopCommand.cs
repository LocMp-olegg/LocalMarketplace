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
    double? Longitude,
    string? City,
    string? Street,
    string? HouseNumber,
    string? Apartment,
    string? Entrance,
    string? Floor
) : IRequest<ShopDto>;
