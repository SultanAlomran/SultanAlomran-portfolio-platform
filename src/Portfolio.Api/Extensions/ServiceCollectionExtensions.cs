using Microsoft.Extensions.Options;
using Portfolio.Api.Common;
using Portfolio.Api.Configuration;
using Portfolio.Application;
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
        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName))
            .Validate(options => options.AllowedOrigins.Length > 0, "At least one CORS origin is required.")
            .ValidateOnStart();
        services.AddCors(options => options.AddPolicy(CorsPolicy, policy =>
            policy.WithOrigins(configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()?.AllowedOrigins ?? [])
                .AllowAnyHeader().AllowAnyMethod()));
        services.AddApplication();
        services.AddInfrastructure();
        return services;
    }

    internal static IApplicationBuilder UseConfiguredCors(this IApplicationBuilder app) => app.UseCors(CorsPolicy);
}
