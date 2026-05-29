using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace LocMp.Order.Application.Orders.Commands.Orders.ApplyCourier;

public sealed class ApplyCourierCommandHandler(OrderDbContext db, IEventBus eventBus, IMapper mapper)
    : IRequestHandler<ApplyCourierCommand, CourierApplicationDto>
{
    public async Task<CourierApplicationDto> Handle(ApplyCourierCommand request, CancellationToken ct)
    {
        var order = await db.Orders
                        .Include(o => o.CourierAssignment)
                        .Include(o => o.CourierApplications)
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (order.DeliveryType != DeliveryType.Delivery)
            throw new ConflictException("Order does not require courier delivery.");

        if (order.Status != OrderStatus.Confirmed)
            throw new ConflictException($"Order must be Confirmed for courier applications (current: {order.Status}).");

        if (order.CourierAssignment is not null)
            throw new ConflictException("A courier is already assigned to this order.");

        if (order.CourierApplications.Any(a =>
                a.CourierId == request.CourierId &&
                a.Status is CourierApplicationStatus.Pending or CourierApplicationStatus.Approved))
            throw new ConflictException("You have already applied for this order.");

        var hasActiveAssignment = await db.CourierAssignments
            .AnyAsync(a => a.CourierId == request.CourierId
                           && a.DeliveredAt == null
                           && a.Order.Status != OrderStatus.Cancelled
                           && a.Order.Status != OrderStatus.Completed, ct);
        if (hasActiveAssignment)
            throw new ConflictException("You already have an active delivery in progress.");

        Point? location = null;
        double? distanceMeters = null;

        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            location = new Point(request.Longitude.Value, request.Latitude.Value) { SRID = 4326 };

            if (order.ShopLocation is not null)
                distanceMeters = await db.Orders
                    .Where(o => o.Id == order.Id)
                    .Select(o => (double?)o.ShopLocation!.Distance(location))
                    .FirstOrDefaultAsync(ct);
        }

        var now = DateTimeOffset.UtcNow;

        var existing = order.CourierApplications.FirstOrDefault(a => a.CourierId == request.CourierId);
        CourierApplication application;

        if (existing is not null)
        {
            // reapplication after Withdrawn/Rejected — reuse the row to satisfy the unique index
            existing.CourierName = request.CourierName;
            existing.CourierPhone = request.CourierPhone;
            existing.CourierLocation = location;
            existing.DistanceToShopMeters = distanceMeters;
            existing.Status = CourierApplicationStatus.Pending;
            existing.AppliedAt = now;
            existing.UpdatedAt = now;
            application = existing;
        }
        else
        {
            application = new CourierApplication(Guid.NewGuid())
            {
                OrderId = order.Id,
                CourierId = request.CourierId,
                CourierName = request.CourierName,
                CourierPhone = request.CourierPhone,
                CourierLocation = location,
                DistanceToShopMeters = distanceMeters,
                Status = CourierApplicationStatus.Pending,
                AppliedAt = now
            };
            db.CourierApplications.Add(application);
        }

        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new CourierApplicationSubmittedEvent(
            application.Id, order.Id, request.CourierId, request.CourierName, now), ct);

        return mapper.Map<CourierApplicationDto>(application);
    }
}