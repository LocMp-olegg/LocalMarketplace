using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.RemoveProductTag;

public sealed record RemoveProductTagCommand(
    Guid ProductId,
    Guid TagId,
    Guid RequesterId,
    bool IsAdmin = false
) : IRequest;
