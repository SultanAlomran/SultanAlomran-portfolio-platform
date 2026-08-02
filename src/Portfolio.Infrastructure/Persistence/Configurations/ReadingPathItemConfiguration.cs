using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class ReadingPathItemConfiguration : IEntityTypeConfiguration<ReadingPathItem>
{
    public void Configure(EntityTypeBuilder<ReadingPathItem> builder)
    {
        builder.ConfigureCommon("ReadingPathItems");
        builder.HasIndex(x => new { x.ReadingPathId, x.Position }).IsUnique(); builder.HasIndex(x => new { x.ReadingPathId, x.EntityType, x.EntityId }).IsUnique(); builder.HasOne(x => x.ReadingPath).WithMany(x => x.Items).HasForeignKey(x => x.ReadingPathId).IsRequired().OnDelete(DeleteBehavior.Cascade); builder.ToTable(t => t.HasCheckConstraint("CK_ReadingPathItems_Position", "[Position] > 0"));
    }
}
