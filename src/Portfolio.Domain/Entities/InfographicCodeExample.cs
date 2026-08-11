using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class InfographicCodeExample : Entity
{
    private InfographicCodeExample() { }
    public static InfographicCodeExample Create(string title, string language, string code, string? filePath,
        int displayOrder) => new()
        {
            Title = title.Trim(),
            Language = language.Trim(),
            Code = code,
            FilePath = string.IsNullOrWhiteSpace(filePath) ? null : filePath.Trim(),
            DisplayOrder = displayOrder
        };
    public Guid InfographicId { get; private set; }
    public string Title { get; private set; } = "";
    public string Language { get; private set; } = "";
    public string Code { get; private set; } = "";
    public string? FilePath { get; private set; }
    public int DisplayOrder { get; private set; }
    public Infographic Infographic { get; private set; } = null!;

}
