using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Domain.Enums;
using LocMp.Catalog.Infrastructure.Persistence;
using LocMp.Contracts.Catalog;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.ReserveStock;

public sealed class ReserveStockCommandHandler(CatalogDbContext db, IEventBus eventBus, IDistributedCache cache)
    : IRequestHandler<ReserveStockCommand>
{
    public async Task Handle(ReserveStockCommand request, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([request.ProductId], ct)
                      ?? throw new NotFoundException($"Product '{request.ProductId}' not found.");

        if (product.IsDeleted || !product.IsActive)
        {
            await eventBus.PublishAsync(new StockReservationFailedEvent(
                request.ProductId, request.OrderId,
                "Product is not available.", DateTimeOffset.UtcNow), ct);
            throw new ConflictException("Product is not available for reservation.");
        }

        if (product.StockQuantity < request.Quantity)
        {
            await eventBus.PublishAsync(new StockReservationFailedEvent(
                request.ProductId, request.OrderId,
                $"Insufficient stock: available {product.StockQuantity}, requested {request.Quantity}.",
                DateTimeOffset.UtcNow), ct);
            throw new ConflictException(
                $"Insufficient stock. Available: {product.StockQuantity}, requested: {request.Quantity}.");
        }

        product.StockQuantity -= request.Quantity;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        db.StockHistory.Add(new StockHistory(Guid.NewGuid())
        {
            ProductId = product.Id,
            ChangeType = StockChangeType.OrderReserved,
            QuantityDelta = -request.Quantity,
            QuantityAfter = product.StockQuantity,
            ReferenceId = request.OrderId,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync($"product:{request.ProductId}", ct);

        await eventBus.PublishAsync(new StockReservedEvent(
            product.Id, request.OrderId, request.Quantity, DateTimeOffset.UtcNow), ct);

        if (product.StockQuantity == 0)
        {
            await eventBus.PublishAsync(new StockDepletedEvent(
                product.Id, product.SellerId, product.Name, DateTimeOffset.UtcNow), ct);
        }
    }
}
