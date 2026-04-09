using LocMp.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Identity.Infrastructure.Persistence.Configurations;

public class ShopProfileConfiguration : IEntityTypeConfiguration<ShopProfile>
{
    public void Configure(EntityTypeBuilder<ShopProfile> builder)
    {
        builder.ToTable("ShopProfiles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.BusinessName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Inn).HasMaxLength(12);
        builder.Property(x => x.BusinessType).IsRequired();
        builder.Property(x => x.WorkingHours).HasMaxLength(200);
        builder.Property(x => x.AvatarUrl).HasMaxLength(1024);
        builder.Property(x => x.AvatarObjectKey).HasMaxLength(512);
        builder.Property(x => x.Location).HasColumnType("geometry(Point, 4326)");

        builder.HasOne(x => x.User)
            .WithMany(u => u.ShopProfiles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
    }
}
