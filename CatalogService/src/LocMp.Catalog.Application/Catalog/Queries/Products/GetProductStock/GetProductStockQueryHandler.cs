using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetProductStock;

public sealed class GetProductStockQueryHandler(CatalogDbContext db)
    : IRequestHandler<GetProductStockQuery, ProductStockDto>
{
    public async Task<ProductStockDto> Handle(GetProductStockQuery request, CancellationToken ct)
    {
        var result = await db.Products
            .Where(p => p.Id == request.ProductId && !p.IsDeleted)
            .Select(p => new { p.StockQuantity, p.IsMadeToOrder, p.LeadTimeDays })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException($"Product '{request.ProductId}' not found.");

        return new ProductStockDto(result.StockQuantity, result.IsMadeToOrder, result.LeadTimeDays);
    }
}
