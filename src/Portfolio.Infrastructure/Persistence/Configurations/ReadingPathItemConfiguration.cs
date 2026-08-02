using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class ReadingPathItemConfiguration : IEntityTypeConfiguration<ReadingPathItem>
{
    public void Configure(EntityTypeBuilder<ReadingPathItem> builder)
    {
        builder.ConfigureCommon("ReadingPathItems");
        EntityRelationships.Configure(builder);
    }
}
