using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Assistant;
using Portfolio.Application.Common.Abstractions;
using Portfolio.Application.Infographics;
using Portfolio.Application.Media;
using Portfolio.Application.Projects;
using Portfolio.Application.TestAnalytics;
using Portfolio.Infrastructure.Assistant;
using Portfolio.Infrastructure.Infographics;
using Portfolio.Infrastructure.Media;
using Portfolio.Infrastructure.Persistence;
using Portfolio.Infrastructure.Projects;
using Portfolio.Infrastructure.TestAnalytics;

namespace Portfolio.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PortfolioDatabase")
            ?? throw new InvalidOperationException("Connection string 'PortfolioDatabase' is required.");
        services.AddDbContext<PortfolioDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(PortfolioDbContext).Assembly.FullName)));
        services.AddScoped<IPortfolioDbContext>(provider => provider.GetRequiredService<PortfolioDbContext>());
        services.AddScoped<IProjectsService, ProjectsService>();
        services.AddSingleton<IAiAssistantClient, DeterministicAiAssistantClient>();
        services.AddScoped<IInfographicsService, InfographicsService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddSingleton<IMediaStorage, LocalMediaStorage>();
        services.AddScoped<ITestAnalyticsService, TestAnalyticsService>();
        services.AddScoped<ITestTelemetryImporter, TestAnalyticsService>();
        services.AddScoped<ITestArtifactContentService, TestArtifactContentService>();
        services.AddHealthChecks().AddDbContextCheck<PortfolioDbContext>(
            "portfolio-database",
            tags: ["ready"]);
        return services;
    }
}
