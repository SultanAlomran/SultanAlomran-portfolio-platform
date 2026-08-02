using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class InfographicCodeExampleConfiguration : IEntityTypeConfiguration<InfographicCodeExample>
{
    public void Configure(EntityTypeBuilder<InfographicCodeExample> builder)
    {
        builder.ConfigureCommon("InfographicCodeExamples");
        builder.HasOne(x => x.Infographic).WithMany(x => x.CodeExamples).HasForeignKey(x => x.InfographicId).IsRequired().OnDelete(DeleteBehavior.Cascade); builder.ToTable(t => t.HasCheckConstraint("CK_InfographicCodeExamples_DisplayOrder", "[DisplayOrder] >= 0"));
    }
}
