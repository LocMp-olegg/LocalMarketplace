using LocMp.BuildingBlocks;

namespace LocMp.Catalog.Domain.Entities;

public class Category(Guid id) : Entity<Guid>(id)
{
    public Guid? ParentCategoryId { get; set; }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual Category? ParentCategory { get; set; }
    public virtual ICollection<Category> SubCategories { get; set; } = [];
    public virtual ICollection<Product> Products { get; set; } = [];
}