using Microsoft.AspNetCore.Http;

namespace LocMp.Catalog.Api.Requests.Categories;

public sealed class UpdateCategoryRequest
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public IFormFile? Image { get; init; }
}
