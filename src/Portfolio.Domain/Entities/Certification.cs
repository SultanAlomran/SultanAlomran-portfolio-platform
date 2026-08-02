using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Certification : AuditableEntity
{
    private Certification() { }
    public string Name { get; private set; } = "";
    public string Issuer { get; private set; } = "";
    public DateOnly IssuedDate { get; private set; }
    public DateOnly? ExpiresDate { get; private set; }
    public string? CredentialId { get; private set; }
    public string? VerificationUrl { get; private set; }
    public Guid? MediaFileId { get; private set; }
    public int DisplayOrder { get; private set; }
    public MediaFile? MediaFile { get; private set; }
    
}
