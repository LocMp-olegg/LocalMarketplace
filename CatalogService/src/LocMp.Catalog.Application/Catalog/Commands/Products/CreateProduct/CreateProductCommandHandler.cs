using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Domain.Enums;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.CreateProduct;

public sealed class CreateProductCommandHandler(CatalogDbContext db)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId && c.IsActive, ct);
        if (!categoryExists)
            throw new NotFoundException($"Category '{request.CategoryId}' not found.");

        Point? location = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
            location = new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 };

        var productId = Guid.NewGuid();
        var product = new Product(productId)
        {
            SellerId = request.SellerId,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Unit = request.Unit,
            StockQuantity = request.InitialStock,
            Location = location,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Products.Add(product);

        if (request.InitialStock > 0)
        {
            db.StockHistory.Add(new StockHistory(Guid.NewGuid())
            {
                ProductId = productId,
                ChangeType = StockChangeType.InitialStock,
                QuantityDelta = request.InitialStock,
                QuantityAfter = request.InitialStock,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }

        await db.SaveChangesAsync(ct);

        return ProductMapper.ToDto(product);
    }
}
