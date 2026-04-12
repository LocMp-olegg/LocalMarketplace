using LocMp.BuildingBlocks.Application.Common;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Queries.GetOrdersAssignedToCourier;

public sealed class GetOrdersAssignedToCourierQueryHandler(OrderDbContext db)
    : IRequestHandler<GetOrdersAssignedToCourierQuery, PagedResult<OrderSummaryDto>>
{
    public async Task<PagedResult<OrderSummaryDto>> Handle(
        GetOrdersAssignedToCourierQuery request, CancellationToken ct)
    {
        var query = db.Orders
            .Include(o => o.Items)
            .Include(o => o.CourierAssignment)
            .Where(o => o.CourierAssignment != null && o.CourierAssignment.CourierId == request.CourierId);

        if (request.StatusFilter.HasValue)
            query = query.Where(o => o.Status == request.StatusFilter.Value);

        var total = await query.CountAsync(ct);

        var orders = await query
            .OrderByDescending(o => o.CourierAssignment!.AssignedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = orders.Select(o => new OrderSummaryDto(
            o.Id, o.BuyerId, o.SellerId,
            o.Status, o.DeliveryType, o.PaymentStatus,
            o.TotalAmount, o.Items.Count,
            o.Items.FirstOrDefault()?.ProductName,
            o.Items.FirstOrDefault()?.MainPhotoUrl,
            o.CreatedAt, o.CompletedAt)).ToList();

        return new PagedResult<OrderSummaryDto>(items, total, request.PageNumber, request.PageSize);
    }
}
