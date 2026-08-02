using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class InfographicStepConfiguration : IEntityTypeConfiguration<InfographicStep>
{
    public void Configure(EntityTypeBuilder<InfographicStep> builder)
    {
        builder.ConfigureCommon("InfographicSteps");
        EntityRelationships.Configure(builder);
    }
}
