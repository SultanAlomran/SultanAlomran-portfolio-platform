using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class InfographicConfiguration : IEntityTypeConfiguration<Infographic>
{
    public void Configure(EntityTypeBuilder<Infographic> builder)
    {
        builder.ConfigureCommon("Infographics");
        EntityRelationships.Configure(builder);
    }
}
