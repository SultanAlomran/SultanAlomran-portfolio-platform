using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class SeriesConfiguration : IEntityTypeConfiguration<Series>
{
    public void Configure(EntityTypeBuilder<Series> builder)
    {
        builder.ConfigureCommon("Series");
        builder.HasQueryFilter(x => !x.IsDeleted); builder.HasIndex(x => x.Slug).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
