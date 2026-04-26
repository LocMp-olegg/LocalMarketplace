using LocMp.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Identity.Infrastructure.Persistence.Configurations;

public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
{
    public void Configure(EntityTypeBuilder<UserAddress> builder)
    {
        builder.ToTable("UserAddresses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title).HasMaxLength(100);
        builder.Property(x => x.City).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Street).HasMaxLength(250).IsRequired();
        builder.Property(x => x.HouseNumber).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Apartment).HasMaxLength(20);
        builder.Property(x => x.Entrance).HasMaxLength(10);
        builder.Property(x => x.Floor).HasMaxLength(10);

        builder.HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Location)
            .HasColumnType("geography");
    }
}