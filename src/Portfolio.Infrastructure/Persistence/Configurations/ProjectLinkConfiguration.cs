using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class ProjectLinkConfiguration : IEntityTypeConfiguration<ProjectLink>
{
    public void Configure(EntityTypeBuilder<ProjectLink> builder)
    {
        builder.ConfigureCommon("ProjectLinks");
        builder.HasOne(x => x.Project).WithMany(x => x.Links).HasForeignKey(x => x.ProjectId).IsRequired().OnDelete(DeleteBehavior.Cascade); builder.ToTable(t => t.HasCheckConstraint("CK_ProjectLinks_DisplayOrder", "[DisplayOrder] >= 0"));
    }
}
