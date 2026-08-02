using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Infographic : SoftDeletableEntity
{
    private Infographic() { }
    public string Title { get; private set; } = "";
    public string Slug { get; private set; } = "";
    public string ShortDescription { get; private set; } = "";
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public DifficultyLevel DifficultyLevel { get; private set; }
    public ContentStatus Status { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public Category Category { get; private set; } = null!;
    public ICollection<InfographicTag> InfographicTags { get; private set; } = new HashSet<InfographicTag>();
    public ICollection<InfographicStep> Steps { get; private set; } = new HashSet<InfographicStep>();
    public ICollection<InfographicResource> Resources { get; private set; } = new HashSet<InfographicResource>();
    public ICollection<InfographicCodeExample> CodeExamples { get; private set; } = new HashSet<InfographicCodeExample>();
    public ICollection<SeriesItem> SeriesItems { get; private set; } = new HashSet<SeriesItem>();
    public void Publish() { Status = ContentStatus.Published; PublishedAt = DateTime.UtcNow; }
    public void Archive() => Status = ContentStatus.Archived;
}
