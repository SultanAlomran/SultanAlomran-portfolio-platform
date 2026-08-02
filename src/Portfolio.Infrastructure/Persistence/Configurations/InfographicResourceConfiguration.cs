using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class InfographicResourceConfiguration : IEntityTypeConfiguration<InfographicResource>
{
    public void Configure(EntityTypeBuilder<InfographicResource> builder)
    {
        builder.ConfigureCommon("InfographicResources");
        builder.HasOne(x => x.Infographic).WithMany(x => x.Resources).HasForeignKey(x => x.InfographicId).IsRequired().OnDelete(DeleteBehavior.Cascade); builder.ToTable(t => t.HasCheckConstraint("CK_InfographicResources_DisplayOrder", "[DisplayOrder] >= 0"));
    }
}
