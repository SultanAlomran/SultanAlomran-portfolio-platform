using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class InfographicConfiguration : IEntityTypeConfiguration<Infographic>
{
    public void Configure(EntityTypeBuilder<Infographic> builder)
    {
        builder.ConfigureCommon("Infographics");
        builder.HasQueryFilter(x => !x.IsDeleted); builder.HasIndex(x => x.Slug).IsUnique().HasFilter("[IsDeleted] = 0"); builder.HasIndex(x => new { x.Status, x.PublishedAt }); builder.HasIndex(x => x.CreatedAt); builder.HasOne(x => x.Category).WithMany(x => x.Infographics).HasForeignKey(x => x.CategoryId).IsRequired().OnDelete(DeleteBehavior.Restrict);
    }
}
