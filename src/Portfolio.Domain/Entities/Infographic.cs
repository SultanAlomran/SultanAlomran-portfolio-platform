using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Infographic : SoftDeletableEntity
{
    private Infographic() { }

    public static Infographic Create(string title, string slug, string shortDescription, Guid categoryId,
        DifficultyLevel difficultyLevel) => new()
        {
            Title = title.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            ShortDescription = shortDescription.Trim(),
            CategoryId = categoryId,
            DifficultyLevel = difficultyLevel,
            Status = ContentStatus.Draft
        };

    public string Title { get; private set; } = "";
    public string Slug { get; private set; } = "";
    public string ShortDescription { get; private set; } = "";
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public DifficultyLevel DifficultyLevel { get; private set; }
    public ContentStatus Status { get; private set; }
    public bool IsFeatured { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public Guid? CoverMediaFileId { get; private set; }
    public Guid? InfographicMediaFileId { get; private set; }
    public Guid? PdfMediaFileId { get; private set; }
    public Category Category { get; private set; } = null!;
    public MediaFile? CoverMediaFile { get; private set; }
    public MediaFile? InfographicMediaFile { get; private set; }
    public MediaFile? PdfMediaFile { get; private set; }
    public ICollection<InfographicTag> InfographicTags { get; private set; } = new HashSet<InfographicTag>();
    public ICollection<InfographicStep> Steps { get; private set; } = new HashSet<InfographicStep>();
    public ICollection<InfographicResource> Resources { get; private set; } = new HashSet<InfographicResource>();
    public ICollection<InfographicCodeExample> CodeExamples { get; private set; } = new HashSet<InfographicCodeExample>();
    public ICollection<SeriesItem> SeriesItems { get; private set; } = new HashSet<SeriesItem>();
    public void UpdateContent(string title, string slug, string shortDescription, string? description,
        Guid categoryId, DifficultyLevel difficultyLevel, bool isFeatured, Guid? coverMediaFileId,
        Guid? infographicMediaFileId, Guid? pdfMediaFileId)
    {
        Title = title.Trim();
        Slug = slug.Trim().ToLowerInvariant();
        ShortDescription = shortDescription.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        CategoryId = categoryId;
        DifficultyLevel = difficultyLevel;
        IsFeatured = isFeatured;
        CoverMediaFileId = coverMediaFileId;
        InfographicMediaFileId = infographicMediaFileId;
        PdfMediaFileId = pdfMediaFileId;
    }

    public void SaveDraft() { Status = ContentStatus.Draft; PublishedAt = null; }
    public void Publish() { Status = ContentStatus.Published; PublishedAt = DateTime.UtcNow; }
    public void Archive() => Status = ContentStatus.Archived;
}
