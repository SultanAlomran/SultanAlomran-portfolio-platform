using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class ReadingPathConfiguration : IEntityTypeConfiguration<ReadingPath>
{
    public void Configure(EntityTypeBuilder<ReadingPath> builder)
    {
        builder.ConfigureCommon("ReadingPaths");
        builder.HasQueryFilter(x => !x.IsDeleted); builder.HasIndex(x => x.Slug).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
