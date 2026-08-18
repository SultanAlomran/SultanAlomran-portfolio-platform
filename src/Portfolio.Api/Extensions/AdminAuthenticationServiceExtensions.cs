using System.Security.Claims;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Portfolio.Api.Configuration;
using Portfolio.Api.Features.Authentication;
using Portfolio.Application.Authentication;

namespace Portfolio.Api.Extensions;

internal static class AdminAuthenticationServiceExtensions
{
    internal static IServiceCollection AddAdminAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var section = configuration.GetSection(AdminAuthenticationOptions.SectionName);
        var configured = section.Get<AdminAuthenticationOptions>() ?? new AdminAuthenticationOptions();
        if (!Uri.TryCreate(configured.AdminBaseUrl, UriKind.Absolute, out var adminUri)
            || (adminUri.Scheme != Uri.UriSchemeHttp && adminUri.Scheme != Uri.UriSchemeHttps))
            throw new InvalidOperationException("Authentication:AdminBaseUrl must be an absolute HTTP(S) URL.");
        if (configured.Google.TestMode && !environment.IsDevelopment())
            throw new InvalidOperationException("Authentication:Google:TestMode is restricted to Development.");

        services.AddOptions<AdminAuthenticationOptions>()
            .Bind(section)
            .Validate(options => options.Cookie.SessionHours is >= 1 and <= 24, "Cookie session lifetime must be 1-24 hours.")
            .Validate(options => options.Cookie.PersistentDays is >= 1 and <= 30, "Persistent cookie lifetime must be 1-30 days.")
            .ValidateOnStart();
        services.AddOptions<AdminBootstrapOptions>().Bind(configuration.GetSection(AdminBootstrapOptions.SectionName));
        services.AddScoped<AdminCookieAuthenticationEvents>();
        services.AddRateLimiter(options => options.AddPolicy("admin-login", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })));
        services.AddCors(options => options.AddPolicy("ConfiguredOrigins", policy =>
            policy.WithOrigins(configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()?.AllowedOrigins ?? [])
                .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

        var securePolicy = Enum.TryParse<CookieSecurePolicy>(configured.Cookie.SecurePolicy, true, out var parsedSecure)
            ? parsedSecure : CookieSecurePolicy.Always;
        var sameSite = Enum.TryParse<SameSiteMode>(configured.Cookie.SameSite, true, out var parsedSameSite)
            ? parsedSameSite : SameSiteMode.Lax;
        var authentication = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = AdminAuthenticationSchemes.ApplicationCookie;
            options.DefaultChallengeScheme = AdminAuthenticationSchemes.ApplicationCookie;
            options.DefaultSignInScheme = AdminAuthenticationSchemes.ApplicationCookie;
        })
        .AddCookie(AdminAuthenticationSchemes.ApplicationCookie, options =>
        {
            options.Cookie.Name = configured.Cookie.Name;
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = securePolicy;
            options.Cookie.SameSite = sameSite;
            options.Cookie.Path = "/";
            options.ExpireTimeSpan = TimeSpan.FromDays(configured.Cookie.PersistentDays);
            options.SlidingExpiration = false;
            options.EventsType = typeof(AdminCookieAuthenticationEvents);
        })
        .AddCookie(AdminAuthenticationSchemes.ExternalCookie, options =>
        {
            options.Cookie.Name = ".Portfolio.Admin.External";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = securePolicy;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        });

        if (configured.Google.Enabled && !configured.Google.TestMode
            && !string.IsNullOrWhiteSpace(configured.Google.ClientId)
            && !string.IsNullOrWhiteSpace(configured.Google.ClientSecret))
            authentication.AddGoogle(AdminAuthenticationSchemes.Google, options =>
            {
                options.ClientId = configured.Google.ClientId;
                options.ClientSecret = configured.Google.ClientSecret;
                options.SignInScheme = AdminAuthenticationSchemes.ExternalCookie;
                options.SaveTokens = false;
                options.Events.OnCreatingTicket = context =>
                {
                    var verified = TryReadGoogleEmailVerified(context.User, "verified_email")
                        || TryReadGoogleEmailVerified(context.User, "email_verified");
                    if (verified && context.Identity is not null
                        && !context.Identity.HasClaim(claim => claim.Type == "google:email_verified"))
                        context.Identity.AddClaim(new Claim("google:email_verified", "true", ClaimValueTypes.Boolean));
                    return Task.CompletedTask;
                };
                options.Events.OnRemoteFailure = context =>
                {
                    context.HandleResponse();
                    context.Response.Redirect($"{configured.AdminBaseUrl.TrimEnd('/')}/login?error=google");
                    return Task.CompletedTask;
                };
            });

        services.AddAuthorization(options => options.AddPolicy(AdminAuthorization.Policy, policy =>
            policy.RequireAuthenticatedUser().RequireRole(AdminAuthorization.Role)));
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = ".Portfolio.Admin.Antiforgery";
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.SecurePolicy = securePolicy;
            options.Cookie.SameSite = sameSite;
        });
        return services;
    }
    private static bool TryReadGoogleEmailVerified(JsonElement user, string propertyName)
    {
        if (!user.TryGetProperty(propertyName, out var value)) return false;
        return value.ValueKind == JsonValueKind.True
            || (value.ValueKind == JsonValueKind.String
                && bool.TryParse(value.GetString(), out var parsed) && parsed);
    }
}
