using LocMp.Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Analytics.Infrastructure.Persistence.Configurations;

public class GeographicActivityConfiguration : IEntityTypeConfiguration<GeographicActivity>
{
    public void Configure(EntityTypeBuilder<GeographicActivity> builder)
    {
        builder.ToTable("GeographicActivities");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ComplexName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.TotalRevenue).HasPrecision(18, 2);
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => new { x.ComplexId, x.PeriodType, x.PeriodStart }).IsUnique();
    }
}
