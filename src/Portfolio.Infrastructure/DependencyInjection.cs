using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Portfolio.Application.Assistant;
using Portfolio.Application.Authentication;
using Portfolio.Application.Common.Abstractions;
using Portfolio.Application.Contact;
using Portfolio.Application.Infographics;
using Portfolio.Application.Media;
using Portfolio.Application.Notifications;
using Portfolio.Application.Projects;
using Portfolio.Application.TestAnalytics;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Assistant;
using Portfolio.Infrastructure.Authentication;
using Portfolio.Infrastructure.Contact;
using Portfolio.Infrastructure.Infographics;
using Portfolio.Infrastructure.Media;
using Portfolio.Infrastructure.Notifications;
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
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAdminAuthenticationService, AdminAuthenticationService>();
        services.AddScoped<IAdminBootstrapService, AdminBootstrapService>();
        services.AddSingleton<IAiAssistantClient, DeterministicAiAssistantClient>();
        services.AddScoped<IInfographicsService, InfographicsService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddSingleton<IMediaStorage, LocalMediaStorage>();
        services.AddScoped<ITestAnalyticsService, TestAnalyticsService>();
        services.AddScoped<ITestTelemetryImporter, TestAnalyticsService>();
        services.AddScoped<ITestArtifactContentService, TestArtifactContentService>();

        // Notifications & Contact
        services.AddSingleton<INotificationQueue, InMemoryNotificationQueue>();
        services.AddScoped<INotificationSettingsService, NotificationSettingsService>();
        services.AddSingleton<DeterministicEmailNotificationService>();
        services.AddSingleton<AzureCommunicationEmailNotificationService>();
        services.AddHttpClient<MetaCloudWhatsAppNotificationService>();
        services.AddSingleton<DeterministicWhatsAppNotificationService>();

        services.AddSingleton<IEmailNotificationService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<NotificationOptions>>().Value;
            var cleanConn = options.Email.ConnectionString?.Trim().Trim('<', '>', '"', '\'');
            if (string.Equals(options.Email.Provider, "AzureCommunicationServices", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(cleanConn))
            {
                return sp.GetRequiredService<AzureCommunicationEmailNotificationService>();
            }
            return sp.GetRequiredService<DeterministicEmailNotificationService>();
        });

        services.AddSingleton<IWhatsAppNotificationService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<NotificationOptions>>().Value;
            var cleanToken = options.WhatsApp.AccessToken?.Trim().Trim('<', '>', '"', '\'');
            var cleanPhoneId = options.WhatsApp.PhoneNumberId?.Trim().Trim('<', '>', '"', '\'');
            if (string.Equals(options.WhatsApp.Provider, "MetaCloud", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(cleanToken) &&
                !string.IsNullOrWhiteSpace(cleanPhoneId))
            {
                return sp.GetRequiredService<MetaCloudWhatsAppNotificationService>();
            }
            return sp.GetRequiredService<DeterministicWhatsAppNotificationService>();
        });

        services.AddHostedService<NotificationBackgroundWorker>();
        services.AddScoped<IContactService, ContactService>();

        services.AddHealthChecks().AddDbContextCheck<PortfolioDbContext>(
            "portfolio-database",
            tags: ["ready"]);
        return services;
    }
}
