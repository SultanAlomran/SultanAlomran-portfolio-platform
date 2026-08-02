using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

/// <summary>Denormalized aggregate cache; interaction records remain authoritative.</summary>
public sealed class EntityStatistic : Entity
{
    private EntityStatistic() { }
    public string EntityType { get; private set; } = "";
    public Guid EntityId { get; private set; }
    public int ViewCount { get; private set; }
    public int UniqueViewCount { get; private set; }
    public int DownloadCount { get; private set; }
    public int ShareCount { get; private set; }
    public int HelpfulCount { get; private set; }
    public decimal RatingAverage { get; private set; }
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    
}
