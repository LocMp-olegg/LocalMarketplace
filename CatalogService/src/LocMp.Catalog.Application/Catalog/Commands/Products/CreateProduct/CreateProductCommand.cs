using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.CreateProduct;

public sealed record CreateProductCommand(
    Guid SellerId,
    Guid? ShopId,
    Guid CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string Unit,
    int InitialStock,
    double? Latitude,
    double? Longitude
) : IRequest<ProductDto>;
