using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class RolePermission : Entity
{
    private RolePermission() { }
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public Role Role { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;
    
}
