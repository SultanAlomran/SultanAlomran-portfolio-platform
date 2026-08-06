using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class ProjectTechnologyConfiguration : IEntityTypeConfiguration<ProjectTechnology>
{
    public void Configure(EntityTypeBuilder<ProjectTechnology> builder)
    {
        builder.ConfigureCommon("ProjectTechnologies");
        builder.HasIndex(x => new { x.ProjectId, x.TechnologyId }).IsUnique(); builder.HasOne(x => x.Project).WithMany(x => x.ProjectTechnologies).HasForeignKey(x => x.ProjectId).IsRequired().OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.Technology).WithMany(x => x.ProjectTechnologies).HasForeignKey(x => x.TechnologyId).IsRequired().OnDelete(DeleteBehavior.Restrict);
    }
}
