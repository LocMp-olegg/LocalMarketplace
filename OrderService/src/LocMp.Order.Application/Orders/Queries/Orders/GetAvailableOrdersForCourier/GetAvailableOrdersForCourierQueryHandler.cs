using LocMp.BuildingBlocks.Application.Common;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocMp.Order.Application.Orders.Queries.Orders.GetAvailableOrdersForCourier;

public sealed class GetAvailableOrdersForCourierQueryHandler(OrderDbContext db)
    : IRequestHandler<GetAvailableOrdersForCourierQuery, PagedResult<OrderSummaryDto>>
{
    public async Task<PagedResult<OrderSummaryDto>> Handle(
        GetAvailableOrdersForCourierQuery request, CancellationToken ct)
    {
        var courierLocation = new Point(request.Longitude, request.Latitude) { SRID = 4326 };
        var radiusMeters = request.RadiusKm * 1000;

        var query = db.Orders
            .Include(o => o.Items)
            .Include(o => o.DeliveryAddress)
            .Include(o => o.CourierAssignment)
            .Where(o =>
                o.DeliveryType == DeliveryType.NeighborCourier &&
                o.Status == OrderStatus.Confirmed &&
                o.CourierAssignment == null &&
                o.DeliveryAddress != null &&
                o.DeliveryAddress.Location != null &&
                o.DeliveryAddress.Location.IsWithinDistance(courierLocation, radiusMeters));

        var total = await query.CountAsync(ct);

        var orders = await query
            .OrderBy(o => o.DeliveryAddress!.Location!.Distance(courierLocation))
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