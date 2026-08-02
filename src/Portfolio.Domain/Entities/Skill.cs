using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Skill : Entity
{
    private Skill() { }
    public Guid SkillCategoryId { get; private set; }
    public string Name { get; private set; } = "";
    public string? Description { get; private set; }
    public int Proficiency { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public SkillCategory SkillCategory { get; private set; } = null!;
    
}
