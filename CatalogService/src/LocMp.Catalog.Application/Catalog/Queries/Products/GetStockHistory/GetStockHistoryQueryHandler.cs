using LocMp.BuildingBlocks.Application.Common;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetStockHistory;

public sealed class GetStockHistoryQueryHandler(CatalogDbContext db)
    : IRequestHandler<GetStockHistoryQuery, PagedResult<StockHistoryDto>>
{
    public async Task<PagedResult<StockHistoryDto>> Handle(
        GetStockHistoryQuery request, CancellationToken ct)
    {
        var product = await db.Products
                          .FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
                      ?? throw new NotFoundException($"Product '{request.ProductId}' not found.");

        if (!request.IsAdmin && product.SellerId != request.RequesterId)
            throw new ForbiddenException("You do not have access to this product's stock history.");

        var query = db.StockHistory.Where(s => s.ProductId == request.ProductId);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new StockHistoryDto(
                s.Id, s.ProductId, s.ChangeType,
                s.QuantityDelta, s.QuantityAfter, s.ReferenceId, s.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<StockHistoryDto>(items, total, request.Page, request.PageSize);
    }
}
