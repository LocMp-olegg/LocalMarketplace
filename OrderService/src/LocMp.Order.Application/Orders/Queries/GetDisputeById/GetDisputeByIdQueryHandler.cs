using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Order.Application.Orders.Queries.GetDisputeById;

public sealed class GetDisputeByIdQueryHandler(OrderDbContext db, IMapper mapper)
    : IRequestHandler<GetDisputeByIdQuery, DisputeDto>
{
    public async Task<DisputeDto> Handle(GetDisputeByIdQuery request, CancellationToken ct)
    {
        var dispute = await db.Disputes
            .Include(d => d.Photos)
            .Include(d => d.Order)
            .FirstOrDefaultAsync(d => d.Id == request.DisputeId, ct)
            ?? throw new NotFoundException($"Dispute '{request.DisputeId}' not found.");

        if (!request.IsAdmin &&
            dispute.Order.BuyerId != request.RequesterId &&
            dispute.Order.SellerId != request.RequesterId &&
            dispute.InitiatorId != request.RequesterId)
            throw new ForbiddenException("You are not a participant in this dispute.");

        return mapper.Map<DisputeDto>(dispute);
    }
}
