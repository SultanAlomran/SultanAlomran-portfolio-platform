using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Role : Entity
{
    private Role() { }
    public string Name { get; private set; } = "";
    public string? Description { get; private set; }
    public ICollection<UserRole> UserRoles { get; private set; } = new HashSet<UserRole>();
    public ICollection<RolePermission> RolePermissions { get; private set; } = new HashSet<RolePermission>();

}
