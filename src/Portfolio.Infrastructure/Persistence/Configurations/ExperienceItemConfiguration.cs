using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class ExperienceItemConfiguration : IEntityTypeConfiguration<ExperienceItem>
{
    public void Configure(EntityTypeBuilder<ExperienceItem> builder)
    {
        builder.ConfigureCommon("ExperienceItems");
        EntityRelationships.Configure(builder);
    }
}
