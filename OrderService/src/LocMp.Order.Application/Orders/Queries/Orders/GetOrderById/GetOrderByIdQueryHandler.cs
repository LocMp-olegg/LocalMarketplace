using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Queries.Orders.GetOrderById;

public sealed class GetOrderByIdQueryHandler(OrderDbContext db, IMapper mapper)
    : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken ct)
    {
        var order = await db.Orders
                        .Include(o => o.Items)
                        .Include(o => o.StatusHistory.OrderBy(h => h.ChangedAt))
                        .Include(o => o.Photos.OrderBy(p => p.SortOrder))
                        .Include(o => o.DeliveryAddress)
                        .Include(o => o.CourierAssignment)
                        .Include(o => o.CourierApplications)
                        .Include(o => o.Dispute).ThenInclude(d => d!.Photos.OrderBy(p => p.SortOrder))
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        var isAssignedCourier = order.CourierAssignment?.CourierId == request.RequesterId;
        var noAssignment = order.CourierAssignment is null;
        var hasActiveApplication = noAssignment && order.CourierApplications.Any(a =>
            a.CourierId == request.RequesterId &&
            a.Status is CourierApplicationStatus.Pending or CourierApplicationStatus.Approved);
        var isAvailableForCourier = request.IsCourier
            && noAssignment
            && order.DeliveryType == DeliveryType.Delivery
            && order.Status == OrderStatus.Confirmed;

        if (!request.IsAdmin
            && order.BuyerId != request.RequesterId
            && order.SellerId != request.RequesterId
            && !isAssignedCourier
            && !hasActiveApplication
            && !isAvailableForCourier)
            throw new ForbiddenException("You are not a participant in this order.");

        return mapper.Map<OrderDto>(order);
    }
}