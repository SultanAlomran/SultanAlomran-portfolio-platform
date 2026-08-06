using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class ProjectImage : Entity
{
    private ProjectImage() { }
    public static ProjectImage Create(Guid mediaFileId, string altText, string? caption, int displayOrder) => new()
    {
        MediaFileId = mediaFileId,
        AltText = altText.Trim(),
        Caption = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim(),
        DisplayOrder = displayOrder
    };
    public Guid ProjectId { get; private set; }
    public Guid MediaFileId { get; private set; }
    public string AltText { get; private set; } = "";
    public string? Caption { get; private set; }
    public int DisplayOrder { get; private set; }
    public Project Project { get; private set; } = null!;
    public MediaFile MediaFile { get; private set; } = null!;

}
