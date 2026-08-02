using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class User : AuditableEntity
{
    private User() { }
    public string UserName { get; private set; } = "";
    public string Email { get; private set; } = "";
    public string PasswordHash { get; private set; } = "";
    public string FullName { get; private set; } = "";
    public bool IsActive { get; private set; } = true;
    public bool EmailVerified { get; private set; }
    public ICollection<UserRole> UserRoles { get; private set; } = new HashSet<UserRole>();
    public ICollection<Session> Sessions { get; private set; } = new HashSet<Session>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new HashSet<RefreshToken>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; private set; } = new HashSet<PasswordResetToken>();
    public ICollection<EmailVerificationToken> EmailVerificationTokens { get; private set; } = new HashSet<EmailVerificationToken>();
    public ICollection<Notification> Notifications { get; private set; } = new HashSet<Notification>();
    public ICollection<AuditLog> AuditLogs { get; private set; } = new HashSet<AuditLog>();
    public ICollection<MediaFile> UploadedMediaFiles { get; private set; } = new HashSet<MediaFile>();

}
