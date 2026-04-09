using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.ReleaseStock;

public sealed record ReleaseStockCommand(Guid ProductId, int Quantity, Guid OrderId) : IRequest;
