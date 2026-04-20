using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NotificationEntity = LocMp.Notification.Domain.Entities.Notification;

namespace LocMp.Notification.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.UserId).IsRequired();
        builder.Property(n => n.Type).IsRequired();
        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Body).HasMaxLength(2000).IsRequired();
        builder.Property(n => n.DeliveryChannel).IsRequired();
        builder.Property(n => n.DeliveryStatus).IsRequired();
        builder.Property(n => n.IsRead).HasDefaultValue(false);
        builder.Property(n => n.Payload).HasColumnType("jsonb");
        builder.Property(n => n.CreatedAt).IsRequired();

        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
    }
}
