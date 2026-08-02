using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ConfigureCommon("Categories");
        builder.HasQueryFilter(x => !x.IsDeleted); builder.HasIndex(x => x.Name).IsUnique().HasFilter("[IsDeleted] = 0"); builder.HasIndex(x => x.Slug).IsUnique().HasFilter("[IsDeleted] = 0"); builder.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
    }
}
