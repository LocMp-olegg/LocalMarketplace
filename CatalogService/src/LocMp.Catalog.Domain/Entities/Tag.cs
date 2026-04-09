using LocMp.BuildingBlocks;

namespace LocMp.Catalog.Domain.Entities;

public class Tag(Guid id) : Entity<Guid>(id)
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;

    public virtual ICollection<ProductTag> ProductTags { get; set; } = [];
}