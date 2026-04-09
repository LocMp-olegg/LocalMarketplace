using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Domain.Enums;
using LocMp.Catalog.Infrastructure.Persistence;
using LocMp.Contracts.Catalog;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.ReleaseStock;

public sealed class ReleaseStockCommandHandler(CatalogDbContext db, IEventBus eventBus, IDistributedCache cache)
    : IRequestHandler<ReleaseStockCommand>
{
    public async Task Handle(ReleaseStockCommand request, CancellationToken ct)
    {
        // Ищем без фильтра на IsDeleted — освобождение нужно даже для удалённых товаров
        var product = await db.Products
                          .FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
                      ?? throw new NotFoundException($"Product '{request.ProductId}' not found.");

        product.StockQuantity += request.Quantity;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        db.StockHistory.Add(new StockHistory(Guid.NewGuid())
        {
            ProductId = product.Id,
            ChangeType = StockChangeType.OrderReleased,
            QuantityDelta = request.Quantity,
            QuantityAfter = product.StockQuantity,
            ReferenceId = request.OrderId,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync($"product:{request.ProductId}", ct);

        await eventBus.PublishAsync(new StockReleasedEvent(
            product.Id, request.OrderId, request.Quantity, DateTimeOffset.UtcNow), ct);
    }
}
