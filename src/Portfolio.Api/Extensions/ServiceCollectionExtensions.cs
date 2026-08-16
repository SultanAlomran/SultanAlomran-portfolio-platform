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
        var assistantRateLimit = configuration.GetSection(AiAssistantOptions.SectionName).Get<AiAssistantOptions>() ?? new();
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("assistant", context => RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
                { PermitLimit = Math.Clamp(assistantRateLimit.RateLimitPermitCount, 1, 100), Window = TimeSpan.FromSeconds(Math.Clamp(assistantRateLimit.RateLimitWindowSeconds, 1, 3_600)), QueueLimit = 0 }));
        });
        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName))
            .Validate(options => options.AllowedOrigins.Length > 0, "At least one CORS origin is required.")
            .ValidateOnStart();
        services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
            policy.WithOrigins(configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()?.AllowedOrigins ?? [])
                .AllowAnyHeader().AllowAnyMethod()));
        services.AddApplication();
        services.AddInfrastructure(configuration);
        return services;
    }

    internal static IApplicationBuilder UseConfiguredCors(this IApplicationBuilder app) => app.UseCors(CorsPolicy);
}
