namespace LocMp.Catalog.Application.DTOs;

public sealed record CategoryDto
{
    public Guid Id { get; init; }
    public Guid? ParentCategoryId { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
