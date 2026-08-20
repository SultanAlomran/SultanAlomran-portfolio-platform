using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portfolio.Application.Authentication;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Authentication;

internal sealed class AdminAuthenticationService(
    PortfolioDbContext db,
    IPasswordHasher<User> passwordHasher,
    ILogger<AdminAuthenticationService> logger) : IAdminAuthenticationService
{
    private readonly User dummyUser = User.Create("timing-probe", "timing-probe@example.invalid", "placeholder", "Timing probe", false);
    private string? dummyHash;

    public async Task<AuthenticationAttempt> AuthenticateLocalAsync(string email, string password, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = normalizedEmail.Length <= 320
            ? await UserQuery().SingleOrDefaultAsync(x => x.Email == normalizedEmail, cancellationToken)
            : null;

        if (user is null)
        {
            dummyHash ??= passwordHasher.HashPassword(dummyUser, "not-a-real-password");
            _ = passwordHasher.VerifyHashedPassword(dummyUser, dummyHash, password);
            return new AuthenticationAttempt(AuthenticationAttemptStatus.InvalidCredentials);
        }

        var verification = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
        {
            await AuditAsync("Auth.LoginFailed", user.Id, cancellationToken);
            logger.LogWarning("A local Admin login attempt failed for user {UserId}.", user.Id);
            return new AuthenticationAttempt(AuthenticationAttemptStatus.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            await AuditAsync("Auth.LoginDenied.Disabled", user.Id, cancellationToken);
            return new AuthenticationAttempt(AuthenticationAttemptStatus.Disabled);
        }

        var authenticated = Project(user);
        if (!authenticated.Roles.Contains(AdminAuthorization.Role, StringComparer.Ordinal))
        {
            await AuditAsync("Auth.LoginDenied.Forbidden", user.Id, cancellationToken);
            return new AuthenticationAttempt(AuthenticationAttemptStatus.Forbidden);
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            user.SetPasswordHash(passwordHasher.HashPassword(user, password));
        db.AuditLogs.Add(AuditLog.Create("Auth.LoginSucceeded.Local", "Authentication", user.Id, user.Id));
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("User {UserId} signed in with local credentials.", user.Id);
        return new AuthenticationAttempt(AuthenticationAttemptStatus.Succeeded, authenticated);
    }

    public async Task<AuthenticationAttempt> AuthenticateExternalAsync(string provider, string providerSubject, CancellationToken cancellationToken)
    {
        var userId = await db.UserExternalLogins
            .Where(x => x.Provider == provider && x.ProviderSubject == providerSubject)
            .Select(x => (Guid?)x.UserId)
            .SingleOrDefaultAsync(cancellationToken);
        if (userId is null)
        {
            db.AuditLogs.Add(AuditLog.Create("Auth.LoginDenied.ExternalIdentity", "Authentication"));
            await db.SaveChangesAsync(cancellationToken);
            logger.LogWarning("An unlinked {Provider} identity was denied Admin access.", provider);
            return new AuthenticationAttempt(AuthenticationAttemptStatus.ExternalIdentityUnknown);
        }

        var user = await UserQuery().SingleAsync(x => x.Id == userId.Value, cancellationToken);
        if (!user.IsActive)
        {
            await AuditAsync("Auth.LoginDenied.Disabled", user.Id, cancellationToken);
            return new AuthenticationAttempt(AuthenticationAttemptStatus.Disabled);
        }

        var authenticated = Project(user);
        if (!authenticated.Roles.Contains(AdminAuthorization.Role, StringComparer.Ordinal))
        {
            await AuditAsync("Auth.LoginDenied.Forbidden", user.Id, cancellationToken);
            return new AuthenticationAttempt(AuthenticationAttemptStatus.Forbidden);
        }

        await AuditAsync($"Auth.LoginSucceeded.{provider}", user.Id, cancellationToken);
        logger.LogInformation("User {UserId} signed in with provider {Provider}.", user.Id, provider);
        return new AuthenticationAttempt(AuthenticationAttemptStatus.Succeeded, authenticated);
    }

    public async Task<AuthenticationAttempt> AuthenticateOrLinkApprovedExternalAsync(
        string provider,
        string providerSubject,
        string? providerEmail,
        bool providerEmailVerified,
        string? approvedEmail,
        CancellationToken cancellationToken)
    {
        var linked = await db.UserExternalLogins.AnyAsync(
            x => x.Provider == provider && x.ProviderSubject == providerSubject,
            cancellationToken);
        if (linked)
            return await AuthenticateExternalAsync(provider, providerSubject, cancellationToken);

        var normalizedProviderEmail = providerEmail?.Trim().ToLowerInvariant();
        var normalizedApprovedEmail = approvedEmail?.Trim().ToLowerInvariant();
        logger.LogInformation("External identity initial-link eligibility: email present {EmailPresent}, verified {EmailVerified}, approved email configured {ApprovedEmailConfigured}.",
            !string.IsNullOrWhiteSpace(normalizedProviderEmail), providerEmailVerified,
            !string.IsNullOrWhiteSpace(normalizedApprovedEmail));

        if (!providerEmailVerified || string.IsNullOrWhiteSpace(normalizedProviderEmail))
            return await AuthenticateExternalAsync(provider, providerSubject, cancellationToken);

        var user = await UserQuery().SingleOrDefaultAsync(
            x => x.Email == normalizedProviderEmail || (normalizedApprovedEmail != null && x.Email == normalizedApprovedEmail),
            cancellationToken);

        if (user is null)
        {
            logger.LogWarning("An unlinked {Provider} identity ({Email}) was denied Admin access because no matching Admin account exists.", provider, normalizedProviderEmail);
            return await AuthenticateExternalAsync(provider, providerSubject, cancellationToken);
        }

        if (!user.IsActive)
        {
            await AuditAsync("Auth.LoginDenied.Disabled", user.Id, cancellationToken);
            return new AuthenticationAttempt(AuthenticationAttemptStatus.Disabled);
        }

        var authenticated = Project(user);
        if (!authenticated.Roles.Contains(AdminAuthorization.Role, StringComparer.Ordinal))
        {
            await AuditAsync("Auth.LoginDenied.Forbidden", user.Id, cancellationToken);
            return new AuthenticationAttempt(AuthenticationAttemptStatus.Forbidden);
        }

        var existingUserLogin = await db.UserExternalLogins.SingleOrDefaultAsync(
            x => x.UserId == user.Id && x.Provider == provider,
            cancellationToken);
        if (existingUserLogin is not null)
        {
            db.UserExternalLogins.Remove(existingUserLogin);
        }

        db.UserExternalLogins.Add(UserExternalLogin.Create(user.Id, provider, providerSubject, normalizedProviderEmail));
        db.AuditLogs.Add(AuditLog.Create($"Auth.ExternalIdentityLinked.{provider}", "Authentication", user.Id, user.Id));
        db.AuditLogs.Add(AuditLog.Create($"Auth.LoginSucceeded.{provider}", "Authentication", user.Id, user.Id));
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("An approved {Provider} identity ({Email}) was linked to user {UserId} and signed in.", provider, normalizedProviderEmail, user.Id);
        return new AuthenticationAttempt(AuthenticationAttemptStatus.Succeeded, authenticated);
    }

    public async Task<AuthenticatedAdmin?> GetCurrentAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await UserQuery().SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null || !user.IsActive) return null;
        var authenticated = Project(user);
        return authenticated.Roles.Contains(AdminAuthorization.Role, StringComparer.Ordinal) ? authenticated : null;
    }

    public async Task RecordLogoutAsync(Guid userId, CancellationToken cancellationToken)
    {
        await AuditAsync("Auth.Logout", userId, cancellationToken);
        logger.LogInformation("User {UserId} signed out.", userId);
    }

    private IQueryable<User> UserQuery() => db.Users
        .Include(x => x.UserRoles)
        .ThenInclude(x => x.Role)
        .ThenInclude(x => x.RolePermissions)
        .ThenInclude(x => x.Permission);

    private static AuthenticatedAdmin Project(User user)
    {
        var roles = user.UserRoles.Select(x => x.Role.Name).Distinct(StringComparer.Ordinal).Order().ToArray();
        var permissions = user.UserRoles.SelectMany(x => x.Role.RolePermissions).Select(x => x.Permission.Name)
            .Distinct(StringComparer.Ordinal).Order().ToArray();
        return new AuthenticatedAdmin(user.Id, user.FullName, user.Email, roles, permissions);
    }

    private async Task AuditAsync(string action, Guid userId, CancellationToken cancellationToken)
    {
        db.AuditLogs.Add(AuditLog.Create(action, "Authentication", userId, userId));
        await db.SaveChangesAsync(cancellationToken);
    }
}
