using LocMp.Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Analytics.Infrastructure.Persistence.Configurations;

public class PlatformDailySummaryConfiguration : IEntityTypeConfiguration<PlatformDailySummary>
{
    public void Configure(EntityTypeBuilder<PlatformDailySummary> builder)
    {
        builder.ToTable("PlatformDailySummaries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.GrossMerchandiseValue).HasPrecision(18, 2);
        builder.Property(x => x.AverageOrderValue).HasPrecision(18, 2);
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.Date).IsUnique();
    }
}
