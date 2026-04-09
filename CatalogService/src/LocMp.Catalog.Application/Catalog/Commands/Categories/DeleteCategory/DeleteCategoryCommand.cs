using MediatR;

namespace LocMp.Catalog.Application.Catalog.Commands.Categories.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest;
