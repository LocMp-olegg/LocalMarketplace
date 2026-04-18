using LocMp.BuildingBlocks.Application.Common;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Queries.Orders.GetAllOrders;

public sealed class GetAllOrdersQueryHandler(OrderDbContext db)
    : IRequestHandler<GetAllOrdersQuery, PagedResult<OrderSummaryDto>>
{
    public async Task<PagedResult<OrderSummaryDto>> Handle(GetAllOrdersQuery request, CancellationToken ct)
    {
        var query = db.Orders.Include(o => o.Items).AsQueryable();

        if (request.StatusFilter.HasValue)
            query = query.Where(o => o.Status == request.StatusFilter.Value);

        if (request.From.HasValue)
            query = query.Where(o => o.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(o => o.CreatedAt <= request.To.Value);

        if (request.MinAmount.HasValue)
            query = query.Where(o => o.TotalAmount >= request.MinAmount.Value);

        if (request.MaxAmount.HasValue)
            query = query.Where(o => o.TotalAmount <= request.MaxAmount.Value);

        var total = await query.CountAsync(ct);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = orders.Select(o => new OrderSummaryDto(
            o.Id, o.CheckoutId, o.BuyerId, o.SellerId, o.SellerName, o.ShopId, o.ShopName,            o.Status, o.DeliveryType, o.PaymentStatus,            o.TotalAmount,
            o.Items.Select(i => new OrderItemDto(
                i.Id, i.ProductId, i.ProductName, i.ProductDescription,                i.MainPhotoUrl, i.ShopId, i.ShopName, i.UnitPrice, i.Quantity, i.Subtotal)).ToList(),
            o.CreatedAt, o.CompletedAt)).ToList();

        return new PagedResult<OrderSummaryDto>(items, total, request.PageNumber, request.PageSize);
    }
}