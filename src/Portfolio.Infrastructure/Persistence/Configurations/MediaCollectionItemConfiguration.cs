using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class MediaCollectionItemConfiguration : IEntityTypeConfiguration<MediaCollectionItem>
{
    public void Configure(EntityTypeBuilder<MediaCollectionItem> builder)
    {
        builder.ConfigureCommon("MediaCollectionItems");
        EntityRelationships.Configure(builder);
    }
}
