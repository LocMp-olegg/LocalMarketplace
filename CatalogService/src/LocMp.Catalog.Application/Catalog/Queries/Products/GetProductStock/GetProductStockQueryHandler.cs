using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetProductStock;

public sealed class GetProductStockQueryHandler(CatalogDbContext db)
    : IRequestHandler<GetProductStockQuery, int>
{
    public async Task<int> Handle(GetProductStockQuery request, CancellationToken ct)
    {
        var stock = await db.Products
            .Where(p => p.Id == request.ProductId && !p.IsDeleted)
            .Select(p => (int?)p.StockQuantity)
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"Product '{request.ProductId}' not found.");

        return stock;
    }
}
