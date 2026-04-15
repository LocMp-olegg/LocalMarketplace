using LocMp.Review.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Review.Infrastructure.Persistence.Configurations;

public class RatingAggregateConfiguration : IEntityTypeConfiguration<RatingAggregate>
{
    public void Configure(EntityTypeBuilder<RatingAggregate> builder)
    {
        builder.ToTable("RatingAggregates");
        builder.HasKey(ra => new { ra.SubjectId, ra.SubjectType });

        builder.Property(ra => ra.AverageRating).HasPrecision(4, 2);
        builder.Property(ra => ra.LastCalculatedAt).IsRequired();
    }
}
