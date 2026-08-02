using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class MediaCollection : Entity
{
    private MediaCollection() { }
    public string Name { get; private set; } = "";
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public ICollection<MediaCollectionItem> Items { get; private set; } = new HashSet<MediaCollectionItem>();
    
}
