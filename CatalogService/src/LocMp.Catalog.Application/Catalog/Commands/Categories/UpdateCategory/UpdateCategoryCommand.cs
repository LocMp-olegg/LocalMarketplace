using LocMp.Catalog.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LocMp.Catalog.Application.Catalog.Commands.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    IFormFile? Image,
    int SortOrder,
    bool IsActive
) : IRequest<CategoryDto>;
