using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

/// <summary>Contains privacy-sensitive telemetry that requires controlled retention and access.</summary>
public sealed class AuditLog : Entity
{
    private AuditLog() { }
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = "";
    public string EntityType { get; private set; } = "";
    public Guid? EntityId { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public User? User { get; private set; }

    public static AuditLog Create(string action, string entityType, Guid? userId = null, Guid? entityId = null)
    {
        if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("Action is required.", nameof(action));
        if (string.IsNullOrWhiteSpace(entityType)) throw new ArgumentException("Entity type is required.", nameof(entityType));
        return new AuditLog
        {
            UserId = userId,
            EntityId = entityId,
            Action = action.Trim(),
            EntityType = entityType.Trim()
        };
    }
}
