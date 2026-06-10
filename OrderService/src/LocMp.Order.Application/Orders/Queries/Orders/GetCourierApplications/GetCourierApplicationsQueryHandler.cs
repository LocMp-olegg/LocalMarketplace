using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Queries.Orders.GetCourierApplications;

public sealed class GetCourierApplicationsQueryHandler(OrderDbContext db, IMapper mapper)
    : IRequestHandler<GetCourierApplicationsQuery, IReadOnlyList<CourierApplicationDto>>
{
    public async Task<IReadOnlyList<CourierApplicationDto>> Handle(
        GetCourierApplicationsQuery request, CancellationToken ct)
    {
        var order = await db.Orders
                        .AsNoTracking()
                        .FirstOrDefaultAsync(o => o.Id == request.OrderId, ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (order.SellerId != request.SellerId)
            throw new ForbiddenException("You are not the seller for this order.");

        var applications = await db.CourierApplications
            .AsNoTracking()
            .Where(a => a.OrderId == request.OrderId)
            .OrderBy(a => a.AppliedAt)
            .ToListAsync(ct);

        return applications.Select(mapper.Map<CourierApplicationDto>).ToList();
    }
}