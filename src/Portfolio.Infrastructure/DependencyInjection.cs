using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Common.Abstractions;
using Portfolio.Application.Projects;
using Portfolio.Infrastructure.Persistence;
using Portfolio.Infrastructure.Projects;

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
        services.AddHealthChecks().AddDbContextCheck<PortfolioDbContext>(
            "portfolio-database",
            tags: ["ready"]);
        return services;
    }
}
