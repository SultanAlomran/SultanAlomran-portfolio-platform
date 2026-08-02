using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class UserRole : Entity
{
    private UserRole() { }
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

}
