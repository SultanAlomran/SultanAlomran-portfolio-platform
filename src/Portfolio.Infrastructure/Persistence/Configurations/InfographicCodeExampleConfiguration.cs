using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class InfographicCodeExampleConfiguration : IEntityTypeConfiguration<InfographicCodeExample>
{
    public void Configure(EntityTypeBuilder<InfographicCodeExample> builder)
    {
        builder.ConfigureCommon("InfographicCodeExamples");
        EntityRelationships.Configure(builder);
    }
}
