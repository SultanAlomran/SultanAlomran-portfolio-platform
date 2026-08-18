namespace Portfolio.Application.Authentication;

public static class AdminAuthorization
{
    public const string Policy = "AdminOnly";
    public const string Role = "Administrator";
    public const string PermissionClaim = "permission";
    public const string ProviderClaim = "authentication_provider";
}

public enum AuthenticationAttemptStatus
{
    Succeeded,
    InvalidCredentials,
    Disabled,
    Forbidden,
    ExternalIdentityUnknown
}

public sealed record AuthenticatedAdmin(
    Guid Id,
    string FullName,
    string Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);

public sealed record AuthenticationAttempt(
    AuthenticationAttemptStatus Status,
    AuthenticatedAdmin? User = null)
{
    public bool Succeeded => Status == AuthenticationAttemptStatus.Succeeded && User is not null;
}

public sealed record AdminBootstrapRequest(
    string Email,
    string UserName,
    string FullName,
    string Password,
    string? GoogleSubject,
    string? GoogleEmail);

public sealed record AdminBootstrapResult(Guid UserId, bool UserCreated, bool RoleAssigned, bool GoogleLinked);

public interface IAdminAuthenticationService
{
    Task<AuthenticationAttempt> AuthenticateLocalAsync(string email, string password, CancellationToken cancellationToken);
    Task<AuthenticationAttempt> AuthenticateExternalAsync(string provider, string providerSubject, CancellationToken cancellationToken);
    Task<AuthenticationAttempt> AuthenticateOrLinkApprovedExternalAsync(
        string provider,
        string providerSubject,
        string? providerEmail,
        bool providerEmailVerified,
        string? approvedEmail,
        CancellationToken cancellationToken);
    Task<AuthenticatedAdmin?> GetCurrentAsync(Guid userId, CancellationToken cancellationToken);
    Task RecordLogoutAsync(Guid userId, CancellationToken cancellationToken);
}

public interface IAdminBootstrapService
{
    Task<AdminBootstrapResult> BootstrapAsync(AdminBootstrapRequest request, CancellationToken cancellationToken);
}
