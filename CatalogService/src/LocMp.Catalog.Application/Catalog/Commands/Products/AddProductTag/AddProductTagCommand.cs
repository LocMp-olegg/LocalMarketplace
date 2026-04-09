using LocMp.Catalog.Application.DTOs;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.AddProductTag;

public sealed record AddProductTagCommand(
    Guid ProductId,
    string TagName,
    Guid RequesterId,
    bool IsAdmin = false
) : IRequest<TagDto>;
