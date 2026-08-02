using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class DailyStat : Entity
{
    private DailyStat() { }
    public DateOnly Date { get; private set; }
    public int VisitorCount { get; private set; }
    public int SessionCount { get; private set; }
    public int PageViewCount { get; private set; }
    public int UniqueUsers { get; private set; }
    public decimal? BounceRate { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;


}
