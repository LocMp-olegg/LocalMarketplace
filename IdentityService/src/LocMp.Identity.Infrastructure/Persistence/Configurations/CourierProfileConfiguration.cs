using LocMp.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Identity.Infrastructure.Persistence.Configurations;

public class CourierProfileConfiguration : IEntityTypeConfiguration<CourierProfile>
{
    public void Configure(EntityTypeBuilder<CourierProfile> builder)
    {
        builder.ToTable("CourierProfiles");

        builder.HasKey(x => x.CourierId);

        builder.Property(x => x.BaseLocation)
            .HasColumnType("geography");

        builder.HasOne(x => x.Courier)
            .WithOne()
            .HasForeignKey<CourierProfile>(x => x.CourierId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}