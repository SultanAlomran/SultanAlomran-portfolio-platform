using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class SkillCategory : Entity
{
    private SkillCategory() { }
    public string Name { get; private set; } = "";
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public ICollection<Skill> Skills { get; private set; } = new HashSet<Skill>();

}
