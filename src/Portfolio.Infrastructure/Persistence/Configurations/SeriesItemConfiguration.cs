using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class SeriesItemConfiguration : IEntityTypeConfiguration<SeriesItem>
{
    public void Configure(EntityTypeBuilder<SeriesItem> builder)
    {
        builder.ConfigureCommon("SeriesItems");
        builder.HasIndex(x => new { x.SeriesId, x.InfographicId }).IsUnique(); builder.HasIndex(x => new { x.SeriesId, x.Position }).IsUnique(); builder.HasOne(x => x.Series).WithMany(x => x.Items).HasForeignKey(x => x.SeriesId).IsRequired().OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.Infographic).WithMany(x => x.SeriesItems).HasForeignKey(x => x.InfographicId).IsRequired().OnDelete(DeleteBehavior.Cascade); builder.ToTable(t => t.HasCheckConstraint("CK_SeriesItems_Position", "[Position] > 0"));
    }
}
