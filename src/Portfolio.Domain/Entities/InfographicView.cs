using Portfolio.Domain.Common;

namespace Portfolio.Domain.Entities;

/// <summary>Represents a privacy-conscious infographic view event tied to an anonymous visitor hash.</summary>
public sealed class InfographicView : Entity
{
    private InfographicView() { }

    public Guid InfographicId { get; private set; }
    public string VisitorKeyHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public Infographic? Infographic { get; private set; }

    public static InfographicView Create(Guid infographicId, string visitorKeyHash)
    {
        if (string.IsNullOrWhiteSpace(visitorKeyHash))
            throw new ArgumentException("Visitor key hash is required.", nameof(visitorKeyHash));

        return new InfographicView
        {
            InfographicId = infographicId,
            VisitorKeyHash = visitorKeyHash,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
