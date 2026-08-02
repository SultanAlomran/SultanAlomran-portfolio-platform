using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class ExperienceItemConfiguration : IEntityTypeConfiguration<ExperienceItem>
{
    public void Configure(EntityTypeBuilder<ExperienceItem> builder)
    {
        builder.ConfigureCommon("ExperienceItems");
        builder.HasIndex(x => new { x.StartDate, x.DisplayOrder }); builder.ToTable(t => { t.HasCheckConstraint("CK_ExperienceItems_DateRange", "[EndDate] IS NULL OR [EndDate] >= [StartDate]"); t.HasCheckConstraint("CK_ExperienceItems_DisplayOrder", "[DisplayOrder] >= 0"); });
    }
}
