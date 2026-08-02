using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ConfigureCommon("Skills");
        builder.HasOne(x => x.SkillCategory).WithMany(x => x.Skills).HasForeignKey(x => x.SkillCategoryId).IsRequired().OnDelete(DeleteBehavior.Restrict); builder.HasIndex(x => new { x.SkillCategoryId, x.Name }).IsUnique(); builder.ToTable(t => t.HasCheckConstraint("CK_Skills_DisplayOrder", "[DisplayOrder] >= 0"));
    }
}
