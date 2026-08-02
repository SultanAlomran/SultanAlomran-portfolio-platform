using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class ReadingPathItem : Entity
{
    private ReadingPathItem() { }
    public Guid ReadingPathId { get; private set; }
    public string EntityType { get; private set; } = "";
    public Guid EntityId { get; private set; }
    public string? Title { get; private set; }
    public int Position { get; private set; }
    public bool IsOptional { get; private set; }
    public ReadingPath ReadingPath { get; private set; } = null!;
    public void SetPosition(int position) { if (position <= 0) throw new ArgumentOutOfRangeException(nameof(position)); Position = position; }
}
