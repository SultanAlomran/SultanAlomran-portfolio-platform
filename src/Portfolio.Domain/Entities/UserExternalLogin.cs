using Portfolio.Domain.Common;

namespace Portfolio.Domain.Entities;

/// <summary>Links an approved local user to a stable external-provider identity. Provider tokens are never stored.</summary>
public sealed class UserExternalLogin : Entity
{
    private UserExternalLogin() { }

    public Guid UserId { get; private set; }
    public string Provider { get; private set; } = "";
    public string ProviderSubject { get; private set; } = "";
    public string? ProviderEmail { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public User User { get; private set; } = null!;

    public static UserExternalLogin Create(Guid userId, string provider, string providerSubject, string? providerEmail = null)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User ID is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(provider)) throw new ArgumentException("Provider is required.", nameof(provider));
        if (string.IsNullOrWhiteSpace(providerSubject)) throw new ArgumentException("Provider subject is required.", nameof(providerSubject));
        return new UserExternalLogin
        {
            UserId = userId,
            Provider = provider.Trim(),
            ProviderSubject = providerSubject.Trim(),
            ProviderEmail = string.IsNullOrWhiteSpace(providerEmail) ? null : providerEmail.Trim().ToLowerInvariant()
        };
    }
}
