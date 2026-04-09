using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Products.GetProductsBySeller;

public sealed record GetProductsBySellerQuery(
    Guid SellerId,
    bool IncludeInactive = false,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductSummaryDto>>;
