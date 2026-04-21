using LocMp.Notification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using NotificationEntity = LocMp.Notification.Domain.Entities.Notification;

namespace LocMp.Notification.Application.Tests;

internal sealed class TestNotificationDbContext(DbContextOptions<NotificationDbContext> options)
    : NotificationDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<NotificationEntity>()
            .Property(n => n.Payload)
            .HasConversion(
                v => v == null ? null : v.RootElement.GetRawText(),
                v => v == null ? null : JsonDocument.Parse(v));
    }
}
