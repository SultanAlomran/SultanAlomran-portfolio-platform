using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class InfographicConfiguration : IEntityTypeConfiguration<Infographic>
{
    public void Configure(EntityTypeBuilder<Infographic> builder)
    {
        builder.ConfigureCommon("Infographics");
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.HasIndex(x => x.Slug).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.Status, x.PublishedAt });
        builder.HasIndex(x => new { x.IsFeatured, x.Status, x.PublishedAt });
        builder.HasIndex(x => x.CreatedAt);
        builder.HasOne(x => x.Category).WithMany(x => x.Infographics).HasForeignKey(x => x.CategoryId).IsRequired().OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CoverMediaFile).WithMany().HasForeignKey(x => x.CoverMediaFileId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.InfographicMediaFile).WithMany().HasForeignKey(x => x.InfographicMediaFileId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PdfMediaFile).WithMany().HasForeignKey(x => x.PdfMediaFileId).OnDelete(DeleteBehavior.Restrict);
    }
}
