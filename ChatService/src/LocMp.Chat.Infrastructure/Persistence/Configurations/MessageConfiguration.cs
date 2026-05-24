using LocMp.Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Chat.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.SenderId).IsRequired();
        builder.Property(m => m.SenderName).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Type).IsRequired();
        builder.Property(m => m.EncryptedBody).IsRequired();
        builder.Property(m => m.SentAt).IsRequired();
        builder.Property(m => m.IsRead).HasDefaultValue(false);
        builder.Property(m => m.IsDeleted).HasDefaultValue(false);
        builder.Property(m => m.ReadAt);
        builder.Property(m => m.DeletedAt);

        builder.HasMany(m => m.Attachments)
            .WithOne(a => a.Message)
            .HasForeignKey(a => a.MessageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.ChatId, m.SentAt });
        builder.HasIndex(m => m.SenderId);
        builder.HasIndex(m => m.IsDeleted);
    }
}