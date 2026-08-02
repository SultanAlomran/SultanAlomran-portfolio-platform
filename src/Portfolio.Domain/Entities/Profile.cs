using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Profile : AuditableEntity
{
    private Profile() { }
    public string SingletonKey { get; private set; } = "primary";
    public string FullName { get; private set; } = "";
    public string? Headline { get; private set; }
    public string? Summary { get; private set; }
    public string? Location { get; private set; }
    public string? Email { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public Guid? ProfileImageMediaFileId { get; private set; }
    public Guid? CvMediaFileId { get; private set; }
    public MediaFile? ProfileImageMediaFile { get; private set; }
    public MediaFile? CvMediaFile { get; private set; }
    
}
