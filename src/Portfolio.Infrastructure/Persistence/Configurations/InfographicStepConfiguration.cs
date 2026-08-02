using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class InfographicStepConfiguration : IEntityTypeConfiguration<InfographicStep>
{
    public void Configure(EntityTypeBuilder<InfographicStep> builder)
    {
        builder.ConfigureCommon("InfographicSteps");
        builder.HasIndex(x => new { x.InfographicId, x.StepNumber }).IsUnique(); builder.HasOne(x => x.Infographic).WithMany(x => x.Steps).HasForeignKey(x => x.InfographicId).IsRequired().OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.MediaFile).WithMany().HasForeignKey(x => x.MediaFileId).OnDelete(DeleteBehavior.Restrict); builder.ToTable(t => t.HasCheckConstraint("CK_InfographicSteps_DisplayOrder", "[DisplayOrder] >= 0"));
    }
}
