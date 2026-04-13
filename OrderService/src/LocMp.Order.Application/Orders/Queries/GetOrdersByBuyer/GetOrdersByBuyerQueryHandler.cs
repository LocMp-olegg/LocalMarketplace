using LocMp.BuildingBlocks.Application.Common;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Queries.GetOrdersByBuyer;

public sealed class GetOrdersByBuyerQueryHandler(OrderDbContext db)
    : IRequestHandler<GetOrdersByBuyerQuery, PagedResult<OrderSummaryDto>>
{
    public async Task<PagedResult<OrderSummaryDto>> Handle(GetOrdersByBuyerQuery request, CancellationToken ct)
    {
        var query = db.Orders
            .Include(o => o.Items)
            .Where(o => o.BuyerId == request.BuyerId);

        if (request.StatusFilter.HasValue)
            query = query.Where(o => o.Status == request.StatusFilter.Value);

        var total = await query.CountAsync(ct);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = orders.Select(o => new OrderSummaryDto(
            o.Id, o.BuyerId, o.SellerId,
            o.Status, o.DeliveryType, o.PaymentStatus,
            o.TotalAmount,
            o.Items.Select(i => new OrderItemDto(
                i.Id, i.ProductId, i.ProductName, i.ProductDescription,
                i.MainPhotoUrl, i.UnitPrice, i.Quantity, i.Subtotal)).ToList(),
            o.CreatedAt, o.CompletedAt)).ToList();

        return new PagedResult<OrderSummaryDto>(items, total, request.PageNumber, request.PageSize);
    }
}