using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class EntityStatisticConfiguration : IEntityTypeConfiguration<EntityStatistic>
{
    public void Configure(EntityTypeBuilder<EntityStatistic> builder)
    {
        builder.ConfigureCommon("EntityStatistics");
        EntityRelationships.Configure(builder);
    }
}
