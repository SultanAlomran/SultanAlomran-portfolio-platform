using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class MediaCollectionItem : Entity
{
    private MediaCollectionItem() { }
    public Guid MediaCollectionId { get; private set; }
    public Guid MediaFileId { get; private set; }
    public int DisplayOrder { get; private set; }
    public MediaCollection MediaCollection { get; private set; } = null!;
    public MediaFile MediaFile { get; private set; } = null!;
    
}
