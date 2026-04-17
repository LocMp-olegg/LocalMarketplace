using LocMp.Analytics.Domain.Entities;
using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.BuildingBlocks.Application.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Analytics.Application.Analytics.Commands;

public sealed record AcknowledgeStockAlertCommand(Guid AlertId, Guid SellerId) : IRequest;

public sealed class AcknowledgeStockAlertCommandHandler(AnalyticsDbContext db)
    : IRequestHandler<AcknowledgeStockAlertCommand>
{
    public async Task Handle(AcknowledgeStockAlertCommand request, CancellationToken ct)
    {
        var alert = await db.StockAlerts
                        .FirstOrDefaultAsync(x => x.Id == request.AlertId && x.SellerId == request.SellerId, ct)
                    ?? throw new NotFoundException(nameof(StockAlert));

        if (alert.IsAcknowledged)
            return;

        alert.IsAcknowledged = true;
        alert.AcknowledgedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}