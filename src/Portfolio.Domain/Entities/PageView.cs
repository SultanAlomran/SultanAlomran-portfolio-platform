using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

/// <summary>Contains privacy-sensitive telemetry that requires controlled retention and access.</summary>
public sealed class PageView : Entity
{
    private PageView() { }
    public string Url { get; private set; } = "";
    public string Title { get; private set; } = "";
    public string? Referrer { get; private set; }
    public Guid? UserId { get; private set; }
    public Guid? SessionId { get; private set; }
    public string? Country { get; private set; }
    public string? Device { get; private set; }
    public string? Browser { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public User? User { get; private set; }
    public Session? Session { get; private set; }
    
}
