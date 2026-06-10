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
        const double locationToleranceMeters = 100; // slack for GPS imprecision

        var courierLocation = new Point(request.Longitude, request.Latitude) { SRID = 4326 };
        var radiusMeters = request.RadiusKm * 1000;

        var query = db.Orders
            .Where(o =>
                o.DeliveryType == DeliveryType.Delivery &&
                o.Status == OrderStatus.Confirmed &&
                o.CourierAssignment == null &&
                o.ShopLocation != null &&
                o.ShopLocation.IsWithinDistance(courierLocation, radiusMeters) &&
                (o.ShopServiceRadiusMeters == null ||
                 o.ShopLocation.IsWithinDistance(courierLocation,
                     (double)o.ShopServiceRadiusMeters + locationToleranceMeters)));

        var total = await query.CountAsync(ct);

        // Distance is projected inside the SQL query so EF Core translates it to
        // ST_Distance(geography, geography) which returns meters, not C# Euclidean degrees.
        var items = await query
            .OrderBy(o => o.ShopLocation!.Distance(courierLocation))
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderSummaryDto(
                o.Id, o.CheckoutId, o.BuyerId, o.SellerId, o.SellerName, o.ShopId, o.ShopName,
                o.Status, o.DeliveryType, o.PaymentStatus, o.TotalAmount,
                o.Items.Select(i => new OrderItemDto(
                    i.Id, i.ProductId, i.ProductName, i.ProductDescription,
                    i.MainPhotoUrl, i.ShopId, i.ShopName, i.UnitPrice, i.Quantity, i.Subtotal)).ToList(),
                o.CreatedAt, o.CompletedAt,
                o.ShopLocation != null ? o.ShopLocation.Distance(courierLocation) : (double?)null))
            .ToListAsync(ct);

        return new PagedResult<OrderSummaryDto>(items, total, request.PageNumber, request.PageSize);
    }
}
