using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Portfolio.Api.Features.Infographics;

internal static class AnonymousEngagementIdentity
{
    internal const string CookieName = ".Portfolio.Engagement";
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(180);

    internal static string GetOrCreateHash(HttpContext context, IHostEnvironment environment)
    {
        var existing = TryGetHash(context);
        if (existing is not null) return existing;

        var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        context.Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            MaxAge = Lifetime,
            Path = "/api/infographics"
        });
        return Hash(token);
    }

    internal static string? TryGetHash(HttpContext context)
    {
        if (!context.Request.Cookies.TryGetValue(CookieName, out var token) ||
            string.IsNullOrWhiteSpace(token) || token.Length > 128)
            return null;
        return Hash(token);
    }

    private static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
