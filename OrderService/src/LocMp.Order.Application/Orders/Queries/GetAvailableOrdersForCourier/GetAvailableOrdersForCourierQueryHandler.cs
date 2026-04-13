using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocMp.Order.Application.Orders.Queries.GetAvailableOrdersForCourier;

public sealed class GetAvailableOrdersForCourierQueryHandler(OrderDbContext db)
    : IRequestHandler<GetAvailableOrdersForCourierQuery, IReadOnlyList<OrderSummaryDto>>
{
    public async Task<IReadOnlyList<OrderSummaryDto>> Handle(
        GetAvailableOrdersForCourierQuery request, CancellationToken ct)
    {
        var courierLocation = new Point(request.Longitude, request.Latitude) { SRID = 4326 };
        var radiusMeters = request.RadiusKm * 1000;

        var orders = await db.Orders
            .Include(o => o.Items)
            .Include(o => o.DeliveryAddress)
            .Include(o => o.CourierAssignment)
            .Where(o =>
                o.DeliveryType == DeliveryType.NeighborCourier &&
                o.Status == OrderStatus.Confirmed &&
                o.CourierAssignment == null &&
                o.DeliveryAddress != null &&
                o.DeliveryAddress.Location != null &&
                o.DeliveryAddress.Location.IsWithinDistance(courierLocation, radiusMeters))
            .OrderBy(o => o.DeliveryAddress!.Location!.Distance(courierLocation))
            .ToListAsync(ct);

        return orders.Select(o => new OrderSummaryDto(
            o.Id, o.BuyerId, o.SellerId,
            o.Status, o.DeliveryType, o.PaymentStatus,
            o.TotalAmount,
            o.Items.Select(i => new OrderItemDto(
                i.Id, i.ProductId, i.ProductName, i.ProductDescription,
                i.MainPhotoUrl, i.UnitPrice, i.Quantity, i.Subtotal)).ToList(),
            o.CreatedAt, o.CompletedAt)).ToList();
    }
}