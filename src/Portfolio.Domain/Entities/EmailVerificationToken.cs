using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

/// <summary>Stores only a cryptographic token hash; raw tokens must never be persisted.</summary>
public sealed class EmailVerificationToken : Entity
{
    private EmailVerificationToken() { }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = "";
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UsedAt { get; private set; }
    public string? IpUsed { get; private set; }
    public User User { get; private set; } = null!;

}
