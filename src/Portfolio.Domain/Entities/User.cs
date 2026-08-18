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
    public ICollection<UserExternalLogin> ExternalLogins { get; private set; } = new HashSet<UserExternalLogin>();

    public static User Create(string userName, string email, string passwordHash, string fullName, bool emailVerified = true)
    {
        if (string.IsNullOrWhiteSpace(userName)) throw new ArgumentException("User name is required.", nameof(userName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name is required.", nameof(fullName));
        return new User
        {
            UserName = userName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName.Trim(),
            EmailVerified = emailVerified
        };
    }

    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        PasswordHash = passwordHash;
    }

    public void SetActive(bool isActive) => IsActive = isActive;
}
