using LocMp.Notification.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Notification.Infrastructure.Persistence.Configurations;

public class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
    {
        builder.ToTable("UserNotificationPreferences");
        builder.HasKey(p => p.UserId);

        builder.Property(p => p.OrderUpdates).HasDefaultValue(true);
        builder.Property(p => p.ReviewReplies).HasDefaultValue(true);
        builder.Property(p => p.SystemAlerts).HasDefaultValue(true);
    }
}
