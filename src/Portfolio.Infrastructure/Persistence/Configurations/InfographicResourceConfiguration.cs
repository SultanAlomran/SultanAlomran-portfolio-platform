using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class InfographicResourceConfiguration : IEntityTypeConfiguration<InfographicResource>
{
    public void Configure(EntityTypeBuilder<InfographicResource> builder)
    {
        builder.ConfigureCommon("InfographicResources");
        EntityRelationships.Configure(builder);
    }
}
