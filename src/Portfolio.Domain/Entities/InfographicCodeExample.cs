using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class InfographicCodeExample : Entity
{
    private InfographicCodeExample() { }
    public Guid InfographicId { get; private set; }
    public string Title { get; private set; } = "";
    public string Language { get; private set; } = "";
    public string Code { get; private set; } = "";
    public string? FilePath { get; private set; }
    public int DisplayOrder { get; private set; }
    public Infographic Infographic { get; private set; } = null!;

}
