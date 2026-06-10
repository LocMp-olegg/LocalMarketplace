using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Interfaces;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Commands.Orders.StartSellerDelivery;

public sealed class StartSellerDeliveryCommandHandler(
    OrderDbContext db,
    ICatalogClient catalogClient,
    IEventBus eventBus)
    : IRequestHandler<StartSellerDeliveryCommand>
{
    public async Task Handle(StartSellerDeliveryCommand request, CancellationToken ct)
    {
        var order = await db.Orders
                        .Include(o => o.CourierAssignment)
                        .Include(o => o.CourierApplications)
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (order.SellerId != request.SellerId)
            throw new ForbiddenException("You are not the seller for this order.");

        if (order.DeliveryType != DeliveryType.Delivery)
            throw new ConflictException("Order does not require delivery.");

        if (order.Status != OrderStatus.Confirmed)
            throw new ConflictException($"Order must be Confirmed to start delivery (current: {order.Status}).");

        if (order.CourierAssignment is not null)
            throw new ConflictException("A courier is already assigned to this order.");

        if (order.ShopId.HasValue)
        {
            var settings = await catalogClient.GetShopDeliverySettingsAsync(order.ShopId.Value, ct);
            if (settings is { AllowSellerDelivery: false })
                throw new ConflictException("Shop does not allow seller-managed delivery.");
        }

        var now = DateTimeOffset.UtcNow;

        var pendingApplications = order.CourierApplications
            .Where(a => a.Status == CourierApplicationStatus.Pending)
            .ToList();

        foreach (var app in pendingApplications)
        {
            app.Status = CourierApplicationStatus.Rejected;
            app.UpdatedAt = now;
        }

        order.IsSellerDelivery = true;

        var (prev, history) = order.TransitionTo(OrderStatus.ReadyForCourier, request.SellerId, now,
            "Seller will deliver the order");
        db.OrderStatusHistory.Add(history);

        await db.SaveChangesAsync(ct);

        await eventBus.PublishAsync(new OrderStatusChangedEvent(
            order.Id, order.BuyerId, order.SellerId,
            prev.ToString(), nameof(OrderStatus.ReadyForCourier), now), ct);

        foreach (var app in pendingApplications)
            await eventBus.PublishAsync(new CourierApplicationRejectedEvent(
                app.Id, order.Id, app.CourierId, now), ct);
    }
}
