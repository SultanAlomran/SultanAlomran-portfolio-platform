using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class DailyStatConfiguration : IEntityTypeConfiguration<DailyStat>
{
    public void Configure(EntityTypeBuilder<DailyStat> builder)
    {
        builder.ConfigureCommon("DailyStats");
        EntityRelationships.Configure(builder);
    }
}
