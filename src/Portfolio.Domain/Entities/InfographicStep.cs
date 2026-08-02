using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class InfographicStep : Entity
{
    private InfographicStep() { }
    public Guid InfographicId { get; private set; }
    public int StepNumber { get; private set; }
    public string Title { get; private set; } = "";
    public string? Content { get; private set; }
    public Guid? MediaFileId { get; private set; }
    public int DisplayOrder { get; private set; }
    public Infographic Infographic { get; private set; } = null!;
    public MediaFile? MediaFile { get; private set; }

}
