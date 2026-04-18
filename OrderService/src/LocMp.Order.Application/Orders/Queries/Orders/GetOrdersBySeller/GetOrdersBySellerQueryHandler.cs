using LocMp.BuildingBlocks.Application.Common;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Enums;
using OrderEntity = LocMp.Order.Domain.Entities.Order;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Queries.Orders.GetOrdersBySeller;

public sealed class GetOrdersBySellerQueryHandler(OrderDbContext db)
    : IRequestHandler<GetOrdersBySellerQuery, PagedResult<OrderSummaryDto>>
{
    public async Task<PagedResult<OrderSummaryDto>> Handle(GetOrdersBySellerQuery request, CancellationToken ct)
    {
        var query = db.Orders
            .Include(o => o.Items)
            .Where(o => o.SellerId == request.SellerId)
            .AsQueryable();

        if (request.ShopId.HasValue)
            query = query.Where(o => o.ShopId == request.ShopId.Value);

        if (request.Statuses is { Count: > 0 })
            query = query.Where(o => request.Statuses.Contains(o.Status));

        if (request.From.HasValue)
            query = query.Where(o => o.CreatedAt >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(o => o.CreatedAt <= request.To.Value);

        if (request.DeliveryType.HasValue)
            query = query.Where(o => o.DeliveryType == request.DeliveryType.Value);

        var total = await query.CountAsync(ct);

        IOrderedQueryable<OrderEntity> sorted = request.SortBy switch
        {
            OrderSortField.Date => request.Descending
                ? query.OrderByDescending(o => o.CreatedAt)
                : query.OrderBy(o => o.CreatedAt),

            OrderSortField.Amount => request.Descending
                ? query.OrderByDescending(o => o.TotalAmount)
                : query.OrderBy(o => o.TotalAmount),

            OrderSortField.Status => query
                .OrderBy(o => (int)o.Status)
                .ThenByDescending(o => o.CreatedAt),

            _ => query
                .OrderBy(o => o.Status == OrderStatus.Pending ? 0 : 1)
                .ThenBy(o => (int)o.Status)
                .ThenByDescending(o => o.CreatedAt)
        };

        var orders = await sorted
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = orders.Select(o => new OrderSummaryDto(
            o.Id, o.CheckoutId, o.BuyerId, o.SellerId, o.SellerName, o.ShopId, o.ShopName,
            o.Status, o.DeliveryType, o.PaymentStatus, o.TotalAmount,
            o.Items.Select(i => new OrderItemDto(
                i.Id, i.ProductId, i.ProductName, i.ProductDescription,
                i.MainPhotoUrl, i.ShopId, i.ShopName, i.UnitPrice, i.Quantity, i.Subtotal)).ToList(),
            o.CreatedAt, o.CompletedAt)).ToList();

        return new PagedResult<OrderSummaryDto>(items, total, request.PageNumber, request.PageSize);
    }
}
