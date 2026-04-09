using Microsoft.AspNetCore.Http;

namespace LocMp.Catalog.Api.Requests.Categories;

public sealed class CreateCategoryRequest
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public Guid? ParentCategoryId { get; init; }
    public int SortOrder { get; init; }
    public IFormFile? Image { get; init; }
}
