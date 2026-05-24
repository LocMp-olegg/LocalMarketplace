using LocMp.Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Chat.Infrastructure.Persistence.Configurations;

public class MessageAttachmentConfiguration : IEntityTypeConfiguration<MessageAttachment>
{
    public void Configure(EntityTypeBuilder<MessageAttachment> builder)
    {
        builder.ToTable("MessageAttachments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName).HasMaxLength(500).IsRequired();
        builder.Property(a => a.MimeType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.MediaType).IsRequired();
        builder.Property(a => a.FileSize).IsRequired();
        builder.Property(a => a.StorageKey).HasMaxLength(1000).IsRequired();
        builder.Property(a => a.UploadedAt).IsRequired();

        builder.HasIndex(a => a.MessageId);
    }
}