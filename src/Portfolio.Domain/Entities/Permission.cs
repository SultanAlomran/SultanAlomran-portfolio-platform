using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Permission : Entity
{
    private Permission() { }
    public string Name { get; private set; } = "";
    public string? Description { get; private set; }
    public ICollection<RolePermission> RolePermissions { get; private set; } = new HashSet<RolePermission>();
    
}
