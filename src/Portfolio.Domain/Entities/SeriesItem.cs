using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class SeriesItem : Entity
{
    private SeriesItem() { }
    public static SeriesItem Create(Guid seriesId, Guid infographicId, int position)
    {
        if (seriesId == Guid.Empty) throw new ArgumentException("Series is required.", nameof(seriesId));
        if (infographicId == Guid.Empty) throw new ArgumentException("Infographic is required.", nameof(infographicId));
        if (position <= 0) throw new ArgumentOutOfRangeException(nameof(position));
        return new SeriesItem { SeriesId = seriesId, InfographicId = infographicId, Position = position };
    }
    public Guid SeriesId { get; private set; }
    public Guid InfographicId { get; private set; }
    public int Position { get; private set; }
    public Series Series { get; private set; } = null!;
    public Infographic Infographic { get; private set; } = null!;
    public void SetPosition(int position) { if (position <= 0) throw new ArgumentOutOfRangeException(nameof(position)); Position = position; }
}
