using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class ProjectLink : Entity
{
    private ProjectLink() { }
    public static ProjectLink Create(string title, string url, string linkType, int displayOrder) => new()
    {
        Title = title.Trim(),
        Url = url.Trim(),
        LinkType = linkType.Trim(),
        DisplayOrder = displayOrder
    };
    public Guid ProjectId { get; private set; }
    public string Title { get; private set; } = "";
    public string Url { get; private set; } = "";
    public string LinkType { get; private set; } = "";
    public int DisplayOrder { get; private set; }
    public Project Project { get; private set; } = null!;

}
