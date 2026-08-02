using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class SeriesItem : Entity
{
    private SeriesItem() { }
    public Guid SeriesId { get; private set; }
    public Guid InfographicId { get; private set; }
    public int Position { get; private set; }
    public Series Series { get; private set; } = null!;
    public Infographic Infographic { get; private set; } = null!;
    public void SetPosition(int position) { if (position <= 0) throw new ArgumentOutOfRangeException(nameof(position)); Position = position; }
}
