using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid Id,
    Guid SellerId,
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Unit,
    bool IsActive,
    bool IsMadeToOrder,
    int? LeadTimeDays,
    double? Latitude,
    double? Longitude
) : IRequest<ProductDto>;