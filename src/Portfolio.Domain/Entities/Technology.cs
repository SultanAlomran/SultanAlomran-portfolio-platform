using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Technology : Entity
{
    private Technology() { }
    public static Technology Create(string name, string category, string? icon = null) => new()
    {
        Name = name.Trim(),
        Category = category.Trim(),
        Icon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim()
    };
    public string Name { get; private set; } = "";
    public string? Icon { get; private set; }
    public string Category { get; private set; } = "";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public ICollection<ProjectTechnology> ProjectTechnologies { get; private set; } = new HashSet<ProjectTechnology>();

}
