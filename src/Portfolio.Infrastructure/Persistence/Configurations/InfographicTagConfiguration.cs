using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class InfographicTagConfiguration : IEntityTypeConfiguration<InfographicTag>
{
    public void Configure(EntityTypeBuilder<InfographicTag> builder)
    {
        builder.ConfigureCommon("InfographicTags");
        EntityRelationships.Configure(builder);
    }
}
