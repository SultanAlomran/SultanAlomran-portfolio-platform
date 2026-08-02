using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class InfographicTagConfiguration : IEntityTypeConfiguration<InfographicTag>
{
    public void Configure(EntityTypeBuilder<InfographicTag> builder)
    {
        builder.ConfigureCommon("InfographicTags");
        builder.HasIndex(x => new { x.InfographicId, x.TagId }).IsUnique(); builder.HasOne(x => x.Infographic).WithMany(x => x.InfographicTags).HasForeignKey(x => x.InfographicId).IsRequired().OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId).IsRequired().OnDelete(DeleteBehavior.Cascade);
    }
}
