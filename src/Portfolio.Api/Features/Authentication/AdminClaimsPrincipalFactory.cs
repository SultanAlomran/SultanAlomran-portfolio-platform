using System.Security.Claims;
using Portfolio.Api.Configuration;
using Portfolio.Application.Authentication;

namespace Portfolio.Api.Features.Authentication;

internal static class AdminClaimsPrincipalFactory
{
    internal static ClaimsPrincipal Create(AuthenticatedAdmin user, string provider)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(AdminAuthorization.ProviderClaim, provider)
        };
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(user.Permissions.Select(permission => new Claim(AdminAuthorization.PermissionClaim, permission)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, AdminAuthenticationSchemes.ApplicationCookie));
    }

    internal static object Response(AuthenticatedAdmin user, string provider) => new
    {
        user.Id,
        user.FullName,
        user.Email,
        user.Roles,
        user.Permissions,
        Provider = provider
    };
}
