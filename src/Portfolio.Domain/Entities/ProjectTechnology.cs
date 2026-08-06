using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class ProjectTechnology : Entity
{
    private ProjectTechnology() { }
    public static ProjectTechnology Create(Guid technologyId) => new() { TechnologyId = technologyId };
    public Guid ProjectId { get; private set; }
    public Guid TechnologyId { get; private set; }
    public Project Project { get; private set; } = null!;
    public Technology Technology { get; private set; } = null!;

}
