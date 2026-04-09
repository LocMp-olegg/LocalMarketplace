using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.ReserveStock;

public sealed record ReserveStockCommand(Guid ProductId, int Quantity, Guid OrderId) : IRequest;
