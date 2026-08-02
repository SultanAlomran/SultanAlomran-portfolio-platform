using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ConfigureCommon("Projects");
        builder.HasQueryFilter(x => !x.IsDeleted); builder.HasIndex(x => x.Slug).IsUnique().HasFilter("[IsDeleted] = 0"); builder.HasIndex(x => new { x.Status, x.PublishedAt }); builder.HasIndex(x => x.CreatedAt); builder.HasOne(x => x.ThumbnailMediaFile).WithMany().HasForeignKey(x => x.ThumbnailMediaFileId).OnDelete(DeleteBehavior.Restrict);
    }
}
