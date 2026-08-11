using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Portfolio.Api.Extensions;
using Portfolio.Api.Features.Projects;
using Portfolio.Api.Features.Infographics;
using Portfolio.Api.Features.TestAnalytics;
using Portfolio.Api.Middleware;
using Portfolio.Infrastructure.Persistence.Seed;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();
builder.Services.AddApiFoundation(builder.Configuration);

var app = builder.Build();
if (args.Contains("--seed-development-projects", StringComparer.OrdinalIgnoreCase))
{
    if (!app.Environment.IsDevelopment())
        throw new InvalidOperationException("Project development seed can run only in the Development environment.");
    var connectionString = builder.Configuration.GetConnectionString("PortfolioDatabase")
        ?? throw new InvalidOperationException("Connection string 'PortfolioDatabase' is required.");
    var result = await DevelopmentProjectSeed.SeedAsync(app.Services, connectionString);
    Console.WriteLine("Development project seed complete: {0} projects, {1} technologies, {2} relationships added.",
        result.ProjectsAdded, result.TechnologiesAdded, result.RelationshipsAdded);
    return;
}
if (args.Contains("--seed-development-infographics", StringComparer.OrdinalIgnoreCase))
{
    if (!app.Environment.IsDevelopment())
        throw new InvalidOperationException("Infographic development seed can run only in the Development environment.");
    var connectionString = builder.Configuration.GetConnectionString("PortfolioDatabase")
        ?? throw new InvalidOperationException("Connection string 'PortfolioDatabase' is required.");
    var result = await DevelopmentInfographicSeed.SeedAsync(app.Services, connectionString);
    Console.WriteLine("Development infographic seed complete: {0} categories, {1} tags, {2} infographics, {3} relationships added.",
        result.CategoriesAdded, result.TagsAdded, result.InfographicsAdded, result.RelationshipsAdded);
    return;
}
app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseConfiguredCors();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options
        .WithTitle("Portfolio Platform API")
        .AddDocument("v1", "Portfolio Platform API")
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
        .DisableAgent());
}
app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = registration => !registration.Tags.Contains("ready")
});
app.MapHealthChecks("/health/ready");
app.MapGet("/api", () => Results.Ok(new { name = "Portfolio.Api", status = "ready" }))
    .WithName("ApiVerification");
app.MapProjectEndpoints();
app.MapInfographicEndpoints();
app.MapTestAnalyticsEndpoints();
app.Run();

public partial class Program;
