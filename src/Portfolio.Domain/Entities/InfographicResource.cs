using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class InfographicResource : Entity
{
    private InfographicResource() { }
    public Guid InfographicId { get; private set; }
    public string Title { get; private set; } = "";
    public string Url { get; private set; } = "";
    public string ResourceType { get; private set; } = "";
    public int DisplayOrder { get; private set; }
    public Infographic Infographic { get; private set; } = null!;

}
