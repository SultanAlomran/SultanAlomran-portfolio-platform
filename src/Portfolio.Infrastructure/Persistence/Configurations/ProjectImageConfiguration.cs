using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class ProjectImageConfiguration : IEntityTypeConfiguration<ProjectImage>
{
    public void Configure(EntityTypeBuilder<ProjectImage> builder)
    {
        builder.ConfigureCommon("ProjectImages");
        builder.HasOne(x => x.Project).WithMany(x => x.Images).HasForeignKey(x => x.ProjectId).IsRequired().OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.MediaFile).WithMany().HasForeignKey(x => x.MediaFileId).IsRequired().OnDelete(DeleteBehavior.Restrict); builder.HasIndex(x => new { x.ProjectId, x.DisplayOrder }).IsUnique(); builder.ToTable(t => t.HasCheckConstraint("CK_ProjectImages_DisplayOrder", "[DisplayOrder] >= 0"));
    }
}
