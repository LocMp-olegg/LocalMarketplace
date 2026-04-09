using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.DeleteProduct;

public sealed class DeleteProductCommandHandler(CatalogDbContext db)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([request.Id], ct)
                      ?? throw new NotFoundException($"Product '{request.Id}' not found.");

        if (product.SellerId != request.SellerId)
            throw new ForbiddenException("You do not own this product.");

        product.IsDeleted = true;
        product.IsActive = false;
        product.DeletedAt = DateTimeOffset.UtcNow;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
