using LocMp.Order.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocMp.Order.Infrastructure.Persistence.Configurations;

public class CourierAssignmentConfiguration : IEntityTypeConfiguration<CourierAssignment>
{
    public void Configure(EntityTypeBuilder<CourierAssignment> builder)
    {
        builder.ToTable("CourierAssignments");
        builder.HasKey(ca => ca.Id);

        builder.Property(ca => ca.CourierName).HasMaxLength(200).IsRequired();
        builder.Property(ca => ca.CourierPhone).HasMaxLength(20).IsRequired();
        builder.Property(ca => ca.AssignedAt).IsRequired();

        builder.HasIndex(ca => ca.OrderId).IsUnique();
        builder.HasIndex(ca => ca.CourierId);
    }
}
