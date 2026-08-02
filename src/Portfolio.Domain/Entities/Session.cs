using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

/// <summary>Contains privacy-sensitive telemetry that requires controlled retention and access.</summary>
public sealed class Session : Entity
{
    private Session() { }
    public string SessionIdentifier { get; private set; } = "";
    public Guid? UserId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? Device { get; private set; }
    public string? Browser { get; private set; }
    public string? Country { get; private set; }
    public DateTime StartedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; private set; }
    public User? User { get; private set; }
    public ICollection<PageView> PageViews { get; private set; } = new HashSet<PageView>();
    public void End(DateTime endedAt) { if (endedAt < StartedAt) throw new ArgumentException("End time cannot precede start time."); EndedAt = endedAt; }
}
