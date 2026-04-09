using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using LocMp.Contracts.Catalog;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.UpdateStock;

public sealed class UpdateStockCommandHandler(CatalogDbContext db, IEventBus eventBus)
    : IRequestHandler<UpdateStockCommand, int>
{
    public async Task<int> Handle(UpdateStockCommand request, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([request.ProductId], ct)
                      ?? throw new NotFoundException($"Product '{request.ProductId}' not found.");

        if (product.SellerId != request.SellerId)
            throw new ForbiddenException("You do not own this product.");

        var newQuantity = product.StockQuantity + request.QuantityDelta;
        if (newQuantity < 0)
            throw new ConflictException($"Insufficient stock. Current: {product.StockQuantity}, delta: {request.QuantityDelta}.");

        product.StockQuantity = newQuantity;
        product.UpdatedAt = DateTimeOffset.UtcNow;

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

        if (newQuantity == 0)
            await eventBus.PublishAsync(new StockDepletedEvent(product.Id, product.SellerId, product.Name, DateTimeOffset.UtcNow), ct);

        return newQuantity;
    }
}
