namespace LocMp.Catalog.Application.DTOs;

public sealed record CategoryDto(
    Guid Id,
    Guid? ParentCategoryId,
    string Name,
    string? Description,
    string? ImageUrl,
    int SortOrder,
    bool IsActive,
    DateTimeOffset CreatedAt
);
