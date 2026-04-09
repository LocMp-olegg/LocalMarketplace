using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id, Guid SellerId) : IRequest;
