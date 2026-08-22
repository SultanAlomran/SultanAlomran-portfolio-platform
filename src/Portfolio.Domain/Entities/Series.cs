using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Series : SoftDeletableEntity
{
    private Series() { }
    public static Series Create(string name, string slug, string? description = null, int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Series name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Series slug is required.", nameof(slug));
        if (displayOrder < 0) throw new ArgumentOutOfRangeException(nameof(displayOrder));
        return new Series
        {
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            DisplayOrder = displayOrder
        };
    }
    public string Name { get; private set; } = "";
    public string Slug { get; private set; } = "";
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public ICollection<SeriesItem> Items { get; private set; } = new HashSet<SeriesItem>();
    public void Activate() => IsActive = true; public void Deactivate() => IsActive = false;
}
