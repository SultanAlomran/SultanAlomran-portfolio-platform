using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class DailyStatConfiguration : IEntityTypeConfiguration<DailyStat>
{
    public void Configure(EntityTypeBuilder<DailyStat> builder)
    {
        builder.ConfigureCommon("DailyStats");
        builder.HasIndex(x => x.Date).IsUnique(); builder.Property(x => x.BounceRate).HasPrecision(5, 2); builder.ToTable(t => { t.HasCheckConstraint("CK_DailyStats_Counters", "[VisitorCount] >= 0 AND [SessionCount] >= 0 AND [PageViewCount] >= 0 AND [UniqueUsers] >= 0"); t.HasCheckConstraint("CK_DailyStats_BounceRate", "[BounceRate] IS NULL OR [BounceRate] BETWEEN 0 AND 100"); });
    }
}
