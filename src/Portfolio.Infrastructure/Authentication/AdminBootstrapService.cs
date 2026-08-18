using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Authentication;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Authentication;

internal sealed class AdminBootstrapService(
    PortfolioDbContext db,
    IPasswordHasher<User> passwordHasher) : IAdminBootstrapService
{
    public async Task<AdminBootstrapResult> BootstrapAsync(AdminBootstrapRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var email = request.Email.Trim().ToLowerInvariant();
        var role = await db.Roles.SingleAsync(x => x.Name == AdminAuthorization.Role, cancellationToken);
        var user = await db.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
        var userCreated = false;
        if (user is null)
        {
            user = User.Create(request.UserName, email, "pending-password-hash", request.FullName);
            user.SetPasswordHash(passwordHasher.HashPassword(user, request.Password));
            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);
            userCreated = true;
        }

        if (!user.IsActive) throw new InvalidOperationException("The configured bootstrap user is inactive.");
        var roleAssigned = !await db.UserRoles.AnyAsync(x => x.UserId == user.Id && x.RoleId == role.Id, cancellationToken);
        if (roleAssigned) db.UserRoles.Add(UserRole.Create(user.Id, role.Id));

        var googleLinked = false;
        if (!string.IsNullOrWhiteSpace(request.GoogleSubject))
        {
            var subject = request.GoogleSubject.Trim();
            var existing = await db.UserExternalLogins.SingleOrDefaultAsync(
                x => x.Provider == "Google" && x.ProviderSubject == subject, cancellationToken);
            if (existing is not null && existing.UserId != user.Id)
                throw new InvalidOperationException("The configured Google identity is already linked to another user.");
            var hasGoogle = await db.UserExternalLogins.AnyAsync(x => x.UserId == user.Id && x.Provider == "Google", cancellationToken);
            if (!hasGoogle)
            {
                db.UserExternalLogins.Add(UserExternalLogin.Create(user.Id, "Google", subject, request.GoogleEmail));
                googleLinked = true;
            }
        }

        if (userCreated || roleAssigned || googleLinked)
            db.AuditLogs.Add(AuditLog.Create("Auth.AdminBootstrapped", "Authentication", user.Id, user.Id));
        await db.SaveChangesAsync(cancellationToken);
        return new AdminBootstrapResult(user.Id, userCreated, roleAssigned, googleLinked);
    }

    private static void Validate(AdminBootstrapRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Length > 320)
            throw new InvalidOperationException("AdminBootstrap:Email is required and must be at most 320 characters.");
        if (string.IsNullOrWhiteSpace(request.UserName) || request.UserName.Length > 100)
            throw new InvalidOperationException("AdminBootstrap:UserName is required and must be at most 100 characters.");
        if (string.IsNullOrWhiteSpace(request.FullName) || request.FullName.Length > 200)
            throw new InvalidOperationException("AdminBootstrap:FullName is required and must be at most 200 characters.");
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 14)
            throw new InvalidOperationException("AdminBootstrap:Password must be supplied securely and contain at least 14 characters.");
        if (request.GoogleSubject?.Length > 255)
            throw new InvalidOperationException("AdminBootstrap:GoogleSubject must be at most 255 characters.");
    }
}
