using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class InfographicResource : Entity
{
    private InfographicResource() { }
    public static InfographicResource Create(string title, string url, string resourceType, int displayOrder) => new()
    {
        Title = title.Trim(),
        Url = url.Trim(),
        ResourceType = resourceType.Trim(),
        DisplayOrder = displayOrder
    };
    public Guid InfographicId { get; private set; }
    public string Title { get; private set; } = "";
    public string Url { get; private set; } = "";
    public string ResourceType { get; private set; } = "";
    public int DisplayOrder { get; private set; }
    public Infographic Infographic { get; private set; } = null!;

}
