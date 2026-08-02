using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

/// <summary>Stores only a cryptographic token hash; raw tokens must never be persisted.</summary>
public sealed class RefreshToken : Entity
{
    private RefreshToken() { }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = "";
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public string? CreatedByIp { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    public string? ReplacedByIp { get; private set; }
    public User User { get; private set; } = null!;
    public RefreshToken? ReplacedByToken { get; private set; }
    public void Revoke() => RevokedAt = DateTime.UtcNow; public void ReplaceWith(Guid replacementId) { Revoke(); ReplacedByTokenId = replacementId; }
}
