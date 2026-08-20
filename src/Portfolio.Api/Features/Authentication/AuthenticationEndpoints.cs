using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Portfolio.Api.Configuration;
using Portfolio.Application.Authentication;

namespace Portfolio.Api.Features.Authentication;

internal static class AuthenticationEndpoints
{
    internal static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Authentication");

        group.MapGet("/csrf", (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            return Results.Ok(new { token = tokens.RequestToken, headerName = tokens.HeaderName });
        });

        group.MapGet("/providers", (IOptions<AdminAuthenticationOptions> options) =>
            Results.Ok(new { google = IsGoogleAvailable(options.Value.Google) }));

        group.MapPost("/login", LoginAsync)
            .RequireRateLimiting("admin-login")
            .AddEndpointFilter<AntiforgeryEndpointFilter>();

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization(AdminAuthorization.Policy)
            .AddEndpointFilter<AntiforgeryEndpointFilter>();

        group.MapGet("/me", CurrentAsync).RequireAuthorization(AdminAuthorization.Policy);
        group.MapGet("/google", BeginGoogleAsync);
        group.MapGet("/google/callback", CompleteGoogleAsync);
        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        HttpContext context,
        IAdminAuthenticationService authenticationService,
        IOptions<AdminAuthenticationOptions> options,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Length > 320
            || string.IsNullOrEmpty(request.Password) || request.Password.Length > 512)
            return InvalidCredentials();

        var attempt = await authenticationService.AuthenticateLocalAsync(request.Email, request.Password, cancellationToken);
        if (!attempt.Succeeded) return InvalidCredentials();
        var user = attempt.User!;
        var properties = Properties(request.RememberMe, options.Value.Cookie);
        await context.SignInAsync(
            AdminAuthenticationSchemes.ApplicationCookie,
            AdminClaimsPrincipalFactory.Create(user, "Local"),
            properties);
        return Results.Ok(AdminClaimsPrincipalFactory.Response(user, "Local"));
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext context,
        IAdminAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        if (Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            await authenticationService.RecordLogoutAsync(userId, cancellationToken);
        await context.SignOutAsync(AdminAuthenticationSchemes.ApplicationCookie);
        await context.SignOutAsync(AdminAuthenticationSchemes.ExternalCookie);
        return Results.NoContent();
    }

    private static async Task<IResult> CurrentAsync(
        HttpContext context,
        IAdminAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Results.Unauthorized();
        var user = await authenticationService.GetCurrentAsync(userId, cancellationToken);
        if (user is null) return Results.Unauthorized();
        var provider = context.User.FindFirstValue(AdminAuthorization.ProviderClaim) ?? "Local";
        return Results.Ok(AdminClaimsPrincipalFactory.Response(user, provider));
    }

    private static async Task<IResult> BeginGoogleAsync(
        string? returnUrl,
        HttpContext context,
        IAdminAuthenticationService authenticationService,
        IOptions<AdminAuthenticationOptions> options,
        IOptions<AdminBootstrapOptions> bootstrapOptions,
        CancellationToken cancellationToken)
    {
        var auth = options.Value;
        var safeReturnUrl = SafeReturnUrl(returnUrl);
        if (!IsGoogleAvailable(auth.Google))
            return Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Google sign-in is not configured.");

        if (auth.Google.TestMode)
        {
            var approvedEmail = !string.IsNullOrWhiteSpace(bootstrapOptions.Value.GoogleEmail)
                ? bootstrapOptions.Value.GoogleEmail
                : bootstrapOptions.Value.Email;
            var testEmail = !string.IsNullOrWhiteSpace(approvedEmail) ? approvedEmail : "admin.e2e@portfolio.test";

            var attempt = await authenticationService.AuthenticateOrLinkApprovedExternalAsync(
                "Google",
                auth.Google.TestSubject,
                testEmail,
                true,
                approvedEmail,
                cancellationToken);

            if (!attempt.Succeeded)
                return Results.Redirect(AdminUrl(auth, $"/login?error=not-authorized&returnUrl={Uri.EscapeDataString(safeReturnUrl)}"));
            var user = attempt.User!;
            await context.SignInAsync(AdminAuthenticationSchemes.ApplicationCookie, AdminClaimsPrincipalFactory.Create(user, "Google"), Properties(false, auth.Cookie));
            return Results.Redirect(AdminUrl(auth, AppendLoginSuccess(safeReturnUrl, "google")));
        }

        var callback = $"/api/auth/google/callback?returnUrl={Uri.EscapeDataString(safeReturnUrl)}";
        return Results.Challenge(new AuthenticationProperties { RedirectUri = callback }, [AdminAuthenticationSchemes.Google]);
    }

    private static async Task<IResult> CompleteGoogleAsync(
        string? returnUrl,
        HttpContext context,
        IAdminAuthenticationService authenticationService,
        IOptions<AdminAuthenticationOptions> options,
        IOptions<AdminBootstrapOptions> bootstrapOptions,
        CancellationToken cancellationToken)
    {
        var auth = options.Value;
        var safeReturnUrl = SafeReturnUrl(returnUrl);
        var external = await context.AuthenticateAsync(AdminAuthenticationSchemes.ExternalCookie);
        if (!external.Succeeded || external.Principal is null)
            return Results.Redirect(AdminUrl(auth, $"/login?error=google&returnUrl={Uri.EscapeDataString(safeReturnUrl)}"));

        var subject = external.Principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? external.Principal.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(subject))
            return Results.Redirect(AdminUrl(auth, $"/login?error=google&returnUrl={Uri.EscapeDataString(safeReturnUrl)}"));

        var email = external.Principal.FindFirstValue(ClaimTypes.Email)
            ?? external.Principal.FindFirstValue("email");

        var googleVerifiedClaim = external.Principal.FindFirstValue("google:email_verified")
            ?? external.Principal.FindFirstValue("email_verified");
        var isEmailVerified = bool.TryParse(googleVerifiedClaim, out var verified) ? verified : !string.IsNullOrWhiteSpace(email);

        var approvedEmail = !string.IsNullOrWhiteSpace(bootstrapOptions.Value.GoogleEmail)
            ? bootstrapOptions.Value.GoogleEmail
            : bootstrapOptions.Value.Email;

        var attempt = await authenticationService.AuthenticateOrLinkApprovedExternalAsync(
            "Google",
            subject,
            email,
            isEmailVerified,
            approvedEmail,
            cancellationToken);

        await context.SignOutAsync(AdminAuthenticationSchemes.ExternalCookie);
        if (!attempt.Succeeded)
            return Results.Redirect(AdminUrl(auth, $"/login?error=not-authorized&returnUrl={Uri.EscapeDataString(safeReturnUrl)}"));

        var user = attempt.User!;
        await context.SignInAsync(AdminAuthenticationSchemes.ApplicationCookie, AdminClaimsPrincipalFactory.Create(user, "Google"), Properties(false, auth.Cookie));
        return Results.Redirect(AdminUrl(auth, AppendLoginSuccess(safeReturnUrl, "google")));
    }

    private static string AppendLoginSuccess(string safeReturnUrl, string provider)
    {
        var separator = safeReturnUrl.Contains('?') ? "&" : "?";
        return $"{safeReturnUrl}{separator}loginSuccess={Uri.EscapeDataString(provider.ToLowerInvariant())}";
    }

    private static AuthenticationProperties Properties(bool rememberMe, AdminAuthenticationOptions.CookieOptions options)
    {
        var lifetime = rememberMe ? TimeSpan.FromDays(options.PersistentDays) : TimeSpan.FromHours(options.SessionHours);
        return new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            AllowRefresh = false,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.Add(lifetime)
        };
    }

    private static bool IsGoogleAvailable(AdminAuthenticationOptions.GoogleOptions options) =>
        options.Enabled && (options.TestMode || (!string.IsNullOrWhiteSpace(options.ClientId) && !string.IsNullOrWhiteSpace(options.ClientSecret)));

    private static string SafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/') || returnUrl.StartsWith("//", StringComparison.Ordinal))
            return "/dashboard";
        return Uri.TryCreate(returnUrl, UriKind.Relative, out _) ? returnUrl : "/dashboard";
    }

    private static string AdminUrl(AdminAuthenticationOptions options, string path) =>
        $"{options.AdminBaseUrl.TrimEnd('/')}{path}";

    private static IResult InvalidCredentials() => Results.Problem(
        statusCode: StatusCodes.Status401Unauthorized,
        title: "Invalid email or password.");

    private sealed record LoginRequest([property: FromBody] string Email, string Password, bool RememberMe);
}
