using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NetTopologySuite.Geometries;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.UpdateProduct;

public sealed class UpdateProductCommandHandler(CatalogDbContext db, IMapper mapper, IDistributedCache cache)
    : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken ct)
    {
        var product = await db.Products
                          .Include(p => p.Photos)
                          .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
                          .FirstOrDefaultAsync(p => p.Id == request.Id, ct)
                      ?? throw new NotFoundException($"Product '{request.Id}' not found.");

        if (product.SellerId != request.SellerId)
            throw new ForbiddenException("You do not own this product.");

        var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId && c.IsActive, ct);
        if (!categoryExists)
            throw new NotFoundException($"Category '{request.CategoryId}' not found.");

        product.CategoryId = request.CategoryId;
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Unit = request.Unit;
        product.IsActive = request.IsActive;
        product.IsMadeToOrder = request.IsMadeToOrder;
        product.LeadTimeDays = request.IsMadeToOrder ? request.LeadTimeDays : null;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        if (request.Latitude.HasValue && request.Longitude.HasValue)
            product.Location = new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 };
        else
            product.Location = null;

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync($"product:{product.Id}", ct);

        return mapper.Map<ProductDto>(product);
    }
}