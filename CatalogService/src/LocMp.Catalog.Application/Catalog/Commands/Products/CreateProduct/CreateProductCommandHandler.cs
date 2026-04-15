using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Domain.Enums;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.CreateProduct;

public sealed class CreateProductCommandHandler(CatalogDbContext db, IMapper mapper)
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken ct)
    {
        var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId && c.IsActive, ct);
        if (!categoryExists)
            throw new NotFoundException($"Category '{request.CategoryId}' not found.");

        if (request.ShopId.HasValue)
        {
            var shop = await db.Shops.FirstOrDefaultAsync(s => s.Id == request.ShopId.Value, ct)
                       ?? throw new NotFoundException($"Shop '{request.ShopId}' not found.");
            if (shop.SellerId != request.SellerId)
                throw new ForbiddenException("You do not own this shop.");
            if (!shop.IsActive)
                throw new ConflictException("Shop is not active.");
        }

        Point? location = null;
        if (request.Latitude.HasValue && request.Longitude.HasValue)
            location = new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 };

        var productId = Guid.NewGuid();
        var product = new Product(productId)
        {
            SellerId = request.SellerId,
            ShopId = request.ShopId,
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Unit = request.Unit,
            StockQuantity = request.InitialStock,
            IsMadeToOrder = request.IsMadeToOrder,
            LeadTimeDays = request.IsMadeToOrder ? request.LeadTimeDays : null,
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

        return mapper.Map<ProductDto>(product);
    }
}