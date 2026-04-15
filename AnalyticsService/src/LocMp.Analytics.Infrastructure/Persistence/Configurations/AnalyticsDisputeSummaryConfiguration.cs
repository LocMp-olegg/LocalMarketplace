using LocMp.Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Analytics.Infrastructure.Persistence.Configurations;

public class AnalyticsDisputeSummaryConfiguration : IEntityTypeConfiguration<DisputeSummary>
{
    public void Configure(EntityTypeBuilder<DisputeSummary> builder)
    {
        builder.ToTable("DisputeSummaries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AverageResolutionMinutes).HasPrecision(10, 2);
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.Date).IsUnique();
    }
}
