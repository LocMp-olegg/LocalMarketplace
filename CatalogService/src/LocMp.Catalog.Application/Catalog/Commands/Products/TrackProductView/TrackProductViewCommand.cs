using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.TrackProductView;

public sealed record TrackProductViewCommand(
    Guid ProductId,
    Guid SellerId,
    string ProductName,
    Guid? ViewerId) : IRequest;
