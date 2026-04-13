using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Order.Application.DTOs;
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
                        .Include(o => o.Dispute).ThenInclude(d => d!.Photos.OrderBy(p => p.SortOrder))
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (!request.IsAdmin && order.BuyerId != request.RequesterId && order.SellerId != request.RequesterId)
            throw new ForbiddenException("You are not a participant in this order.");

        return mapper.Map<OrderDto>(order);
    }
}