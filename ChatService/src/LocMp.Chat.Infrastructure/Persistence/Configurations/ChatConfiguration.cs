using LocMp.Chat.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ChatEntity = LocMp.Chat.Domain.Entities.Chat;

namespace LocMp.Chat.Infrastructure.Persistence.Configurations;

public class ChatConfiguration : IEntityTypeConfiguration<ChatEntity>
{
    public void Configure(EntityTypeBuilder<ChatEntity> builder)
    {
        builder.ToTable("Chats");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Type).IsRequired();
        builder.Property(c => c.Status).HasDefaultValue(ChatStatus.Active).IsRequired();
        builder.Property(c => c.EncryptionKey).HasMaxLength(200).IsRequired();
        builder.Property(c => c.InitiatorName).HasMaxLength(200);
        builder.Property(c => c.TargetName).HasMaxLength(200);
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.ReferenceId);
        builder.Property(c => c.LastMessageAt);
        builder.Property(c => c.ClosedAt);

        builder.HasMany(c => c.Participants)
            .WithOne(p => p.Chat)
            .HasForeignKey(p => p.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Chat)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.Type);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.ReferenceId);
        builder.HasIndex(c => c.LastMessageAt);
    }
}