using LocMp.Catalog.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LocMp.Catalog.Application.Catalog.Commands.Categories.CreateCategory;

public sealed record CreateCategoryCommand(
    Guid? ParentCategoryId,
    string Name,
    string? Description,
    IFormFile? Image,
    int SortOrder = 0
) : IRequest<CategoryDto>;
