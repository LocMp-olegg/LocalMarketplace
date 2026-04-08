using LocMp.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Identity.Infrastructure.Persistence.Configurations;

public class UserPhotoConfiguration : IEntityTypeConfiguration<UserPhoto>
{
    public void Configure(EntityTypeBuilder<UserPhoto> builder)
    {
        builder.ToTable("UserPhotos", "media");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PhotoData).IsRequired();
        builder.Property(p => p.MimeType).HasMaxLength(50).IsRequired();
        builder.Property(p => p.FileSize).IsRequired();
        builder.Property(p => p.UploadedAt).IsRequired();

        builder.HasOne(p => p.User)
            .WithOne(u => u.Photo)
            .HasForeignKey<UserPhoto>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.UserId).IsUnique();
    }
}