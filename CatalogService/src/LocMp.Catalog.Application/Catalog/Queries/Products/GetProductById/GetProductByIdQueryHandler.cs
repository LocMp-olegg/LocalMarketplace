using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using LocMp.Contracts.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetProductById;

public sealed class GetProductByIdQueryHandler(CatalogDbContext db, IMapper mapper, IEventBus eventBus)
    : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken ct)
    {
        var product = await db.Products
                          .Include(p => p.Photos)
                          .Include(p => p.ProductTags).ThenInclude(pt => pt.Tag)
                          .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, ct)
                      ?? throw new NotFoundException($"Product '{request.Id}' not found.");

        if (!product.IsActive && !request.IsAdmin && product.SellerId != request.ViewerId)
            throw new NotFoundException($"Product '{request.Id}' not found.");

        await eventBus.PublishAsync(
            new ProductViewedEvent(product.Id, product.SellerId, request.ViewerId, DateTimeOffset.UtcNow), ct);

        return mapper.Map<ProductDto>(product);
    }
}