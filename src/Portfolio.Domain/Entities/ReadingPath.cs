using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class ReadingPath : SoftDeletableEntity
{
    private ReadingPath() { }
    public string Title { get; private set; } = "";
    public string Slug { get; private set; } = "";
    public string? Description { get; private set; }
    public DifficultyLevel Level { get; private set; }
    public string? Icon { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }
    public ICollection<ReadingPathItem> Items { get; private set; } = new HashSet<ReadingPathItem>();
    public void Activate() => IsActive = true; public void Deactivate() => IsActive = false;
}
