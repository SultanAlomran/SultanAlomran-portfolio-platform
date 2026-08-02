using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class MediaCollectionConfiguration : IEntityTypeConfiguration<MediaCollection>
{
    public void Configure(EntityTypeBuilder<MediaCollection> builder)
    {
        builder.ConfigureCommon("MediaCollections");
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
