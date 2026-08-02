using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Project : SoftDeletableEntity
{
    private Project() { }
    public string Title { get; private set; } = "";
    public string Slug { get; private set; } = "";
    public string ShortDescription { get; private set; } = "";
    public string? Description { get; private set; }
    public Guid? ThumbnailMediaFileId { get; private set; }
    public string? LiveUrl { get; private set; }
    public ContentStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public MediaFile? ThumbnailMediaFile { get; private set; }
    public ICollection<ProjectTechnology> ProjectTechnologies { get; private set; } = new HashSet<ProjectTechnology>();
    public ICollection<ProjectImage> Images { get; private set; } = new HashSet<ProjectImage>();
    public ICollection<ProjectLink> Links { get; private set; } = new HashSet<ProjectLink>();
    public void Publish() { Status = ContentStatus.Published; PublishedAt = DateTime.UtcNow; }
    public void Archive() => Status = ContentStatus.Archived;
}
