using LocMp.Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Chat.Infrastructure.Persistence.Configurations;

public class ChatParticipantConfiguration : IEntityTypeConfiguration<ChatParticipant>
{
    public void Configure(EntityTypeBuilder<ChatParticipant> builder)
    {
        builder.ToTable("ChatParticipants");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId).IsRequired();
        builder.Property(p => p.Role).IsRequired();
        builder.Property(p => p.JoinedAt).IsRequired();
        builder.Property(p => p.LastReadAt);

        builder.HasIndex(p => new { p.ChatId, p.UserId }).IsUnique();
        builder.HasIndex(p => p.UserId);
    }
}