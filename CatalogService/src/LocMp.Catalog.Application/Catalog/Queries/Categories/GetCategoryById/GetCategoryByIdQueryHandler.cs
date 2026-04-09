using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;

namespace LocMp.Catalog.Application.Catalog.Queries.Categories.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler(CatalogDbContext db, IMapper mapper)
    : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync([request.Id], ct)
                       ?? throw new NotFoundException($"Category '{request.Id}' not found.");

        return mapper.Map<CategoryDto>(category);
    }
}