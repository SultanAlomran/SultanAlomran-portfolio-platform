using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class PopularSearchConfiguration : IEntityTypeConfiguration<PopularSearch>
{
    public void Configure(EntityTypeBuilder<PopularSearch> builder)
    {
        builder.ConfigureCommon("PopularSearches");
        EntityRelationships.Configure(builder);
    }
}
