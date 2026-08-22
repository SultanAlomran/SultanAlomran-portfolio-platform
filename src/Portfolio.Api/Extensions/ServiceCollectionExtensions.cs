using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;
using Portfolio.Api.Common;
using Portfolio.Api.Configuration;
using Portfolio.Application;
using Portfolio.Application.Assistant;
using Portfolio.Infrastructure;

namespace Portfolio.Api.Extensions;

internal static class ServiceCollectionExtensions
{
    private const string CorsPolicy = "ConfiguredOrigins";

    internal static IServiceCollection AddApiFoundation(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
            context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier);
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHealthChecks();
        services.AddOpenApi();
        services.AddOptions<AiAssistantOptions>().Bind(configuration.GetSection(AiAssistantOptions.SectionName));
        services.AddOptions<Portfolio.Application.Notifications.NotificationOptions>()
            .Bind(configuration.GetSection(Portfolio.Application.Notifications.NotificationOptions.SectionName));
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("assistant", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
                { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            options.AddPolicy("ai-summary", context => RateLimitPartition.GetFixedWindowLimiter(
                AnonymousEngagementPartition(context), _ => new FixedWindowRateLimiterOptions
                { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            options.AddPolicy("contact-submission", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
                { PermitLimit = 5, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
            options.AddPolicy("engagement-write", context => RateLimitPartition.GetFixedWindowLimiter(
                AnonymousEngagementPartition(context), _ => new FixedWindowRateLimiterOptions
                { PermitLimit = 20, Window = TimeSpan.FromMinutes(5), QueueLimit = 0 }));
        });
        services.AddSignalR();
        services.AddSingleton<Portfolio.Application.Notifications.IAdminRealtimeNotifier, Features.Notifications.SignalRAdminRealtimeNotifier>();
        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName))
            .Validate(options => options.AllowedOrigins.Length > 0, "At least one CORS origin is required.")
            .ValidateOnStart();
        services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
            policy.WithOrigins(configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()?.AllowedOrigins ?? [])
                .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
        services.AddApplication();
        services.AddInfrastructure(configuration);
        return services;
    }

    internal static IApplicationBuilder UseConfiguredCors(this IApplicationBuilder app) => app.UseCors(CorsPolicy);

    private static string AnonymousEngagementPartition(HttpContext context)
    {
        var cookie = context.Request.Cookies[Features.Infographics.AnonymousEngagementIdentity.CookieName];
        if (!string.IsNullOrWhiteSpace(cookie) && cookie.Length <= 128)
            return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(cookie)));
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
