using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

/// <summary>Contains privacy-sensitive telemetry that requires controlled retention and access.</summary>
public sealed class UserInteraction : Entity
{
    private UserInteraction() { }
    public Guid? UserId { get; private set; }
    public string EntityType { get; private set; } = "";
    public Guid EntityId { get; private set; }
    public InteractionType InteractionType { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public User? User { get; private set; }

}
