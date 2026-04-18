using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Catalog;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.TrackProductView;

public sealed class TrackProductViewCommandHandler(IEventBus eventBus)
    : IRequestHandler<TrackProductViewCommand>
{
    public Task Handle(TrackProductViewCommand request, CancellationToken ct)
        => eventBus.PublishAsync(
            new ProductViewedEvent(
                request.ProductId,
                request.SellerId,
                request.ProductName,
                request.ViewerId,
                DateTimeOffset.UtcNow),
            ct);
}
