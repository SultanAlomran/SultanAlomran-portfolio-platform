using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class SeriesItemConfiguration : IEntityTypeConfiguration<SeriesItem>
{
    public void Configure(EntityTypeBuilder<SeriesItem> builder)
    {
        builder.ConfigureCommon("SeriesItems");
        EntityRelationships.Configure(builder);
    }
}
