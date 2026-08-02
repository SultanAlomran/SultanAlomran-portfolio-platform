using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class EntityStatisticConfiguration : IEntityTypeConfiguration<EntityStatistic>
{
    public void Configure(EntityTypeBuilder<EntityStatistic> builder)
    {
        builder.ConfigureCommon("EntityStatistics");
        builder.HasIndex(x => new { x.EntityType, x.EntityId }).IsUnique(); builder.Property(x => x.RatingAverage).HasPrecision(5, 2); builder.ToTable(t => { t.HasCheckConstraint("CK_EntityStatistics_Counters", "[ViewCount] >= 0 AND [UniqueViewCount] >= 0 AND [DownloadCount] >= 0 AND [ShareCount] >= 0 AND [HelpfulCount] >= 0"); t.HasCheckConstraint("CK_EntityStatistics_RatingAverage", "[RatingAverage] BETWEEN 0 AND 5"); });
    }
}
