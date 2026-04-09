using LocMp.Catalog.Domain.Enums;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.UpdateStock;

public sealed record UpdateStockCommand(
    Guid ProductId,
    Guid SellerId,
    int QuantityDelta,
    StockChangeType ChangeType,
    Guid? ReferenceId = null
) : IRequest<int>;
