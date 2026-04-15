using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetProductStock;

public sealed record GetProductStockQuery(Guid ProductId) : IRequest<ProductStockDto>;
