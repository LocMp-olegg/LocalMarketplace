using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using LocMp.Contracts.Catalog;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.UpdateStock;

public sealed class UpdateStockCommandHandler(CatalogDbContext db, IEventBus eventBus, IDistributedCache cache)
    : IRequestHandler<UpdateStockCommand, int>
{
    public async Task<int> Handle(UpdateStockCommand request, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([request.ProductId], ct);
        if (product is null || product.IsDeleted)
            throw new NotFoundException($"Product '{request.ProductId}' not found.");

        if (product.SellerId != request.SellerId)
            throw new ForbiddenException("You do not own this product.");

        int newQuantity;
        try
        {
            newQuantity = product.AdjustStock(request.QuantityDelta);
        }
        catch (InvalidOperationException ex)
        {
            throw new ConflictException(ex.Message);
        }

        db.StockHistory.Add(new StockHistory(Guid.NewGuid())
        {
            ProductId = product.Id,
            ChangeType = request.ChangeType,
            QuantityDelta = request.QuantityDelta,
            QuantityAfter = newQuantity,
            ReferenceId = request.ReferenceId,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync($"product:{product.Id}", ct);

        if (newQuantity == 0)
            await eventBus.PublishAsync(new StockDepletedEvent(product.Id, product.SellerId, product.Name, DateTimeOffset.UtcNow), ct);

        return newQuantity;
    }
}
