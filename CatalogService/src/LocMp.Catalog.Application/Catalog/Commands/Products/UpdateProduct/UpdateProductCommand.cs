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
    double? Latitude,
    double? Longitude,
    bool IsActive
) : IRequest<ProductDto>;