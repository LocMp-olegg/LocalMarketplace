using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetStockHistory;

public sealed record GetStockHistoryQuery(
    Guid ProductId,
    Guid RequesterId,
    bool IsAdmin,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<StockHistoryDto>>;
