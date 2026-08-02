using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Series : SoftDeletableEntity
{
    private Series() { }
    public string Name { get; private set; } = "";
    public string Slug { get; private set; } = "";
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public ICollection<SeriesItem> Items { get; private set; } = new HashSet<SeriesItem>();
    public void Activate() => IsActive = true; public void Deactivate() => IsActive = false;
}
