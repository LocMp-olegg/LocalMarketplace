namespace LocMp.Catalog.Application.DTOs;

public sealed record CategoryTreeDto(
    Guid Id,
    Guid? ParentCategoryId,
    string Name,
    string? Description,
    string? ImageUrl,
    int SortOrder,
    bool IsActive,
    IReadOnlyList<CategoryTreeDto> Children
);
