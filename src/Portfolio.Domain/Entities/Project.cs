using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Project : SoftDeletableEntity
{
    private Project() { }

    public static Project Create(string title, string slug, string shortDescription) => new()
    {
        Title = title.Trim(),
        Slug = slug.Trim().ToLowerInvariant(),
        ShortDescription = shortDescription.Trim(),
        Status = ContentStatus.Draft
    };

    public string Title { get; private set; } = "";
    public string Slug { get; private set; } = "";
    public string ShortDescription { get; private set; } = "";
    public string? Description { get; private set; }
    public string? BusinessProblem { get; private set; }
    public string? Solution { get; private set; }
    public string? Architecture { get; private set; }
    public string? KeyFeatures { get; private set; }
    public string? Challenges { get; private set; }
    public string? Impact { get; private set; }
    public string? LessonsLearned { get; private set; }
    public Guid? ThumbnailMediaFileId { get; private set; }
    public string? LiveUrl { get; private set; }
    public bool IsFeatured { get; private set; }
    public ContentStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public MediaFile? ThumbnailMediaFile { get; private set; }
    public ICollection<ProjectTechnology> ProjectTechnologies { get; private set; } = new HashSet<ProjectTechnology>();
    public ICollection<ProjectImage> Images { get; private set; } = new HashSet<ProjectImage>();
    public ICollection<ProjectLink> Links { get; private set; } = new HashSet<ProjectLink>();
    public void UpdateContent(
        string title,
        string slug,
        string shortDescription,
        string? description,
        string? businessProblem,
        string? solution,
        string? architecture,
        string? keyFeatures,
        string? challenges,
        string? impact,
        string? lessonsLearned,
        Guid? thumbnailMediaFileId,
        string? liveUrl)
    {
        Title = title.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        ShortDescription = shortDescription.Trim();
        Description = Normalize(description);
        BusinessProblem = Normalize(businessProblem);
        Solution = Normalize(solution);
        Architecture = Normalize(architecture);
        KeyFeatures = Normalize(keyFeatures);
        Challenges = Normalize(challenges);
        Impact = Normalize(impact);
        LessonsLearned = Normalize(lessonsLearned);
        ThumbnailMediaFileId = thumbnailMediaFileId;
        LiveUrl = Normalize(liveUrl);
    }

    public void SaveDraft() { Status = ContentStatus.Draft; PublishedAt = null; }
    public void Publish() { Status = ContentStatus.Published; PublishedAt = DateTime.UtcNow; }
    public void Archive() => Status = ContentStatus.Archived;
    public void SetFeatured(bool featured) => IsFeatured = featured;

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
