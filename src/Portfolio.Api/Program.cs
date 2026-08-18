using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Portfolio.Api.Configuration;
using Portfolio.Api.Features.Authentication;
using Portfolio.Application.Authentication;
using Portfolio.Infrastructure.Persistence;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Portfolio.Api.Extensions;
using Portfolio.Api.Features.Assistant;
using Portfolio.Api.Features.Infographics;
using Portfolio.Api.Features.Media;
using Portfolio.Api.Features.Projects;
using Portfolio.Api.Features.TestAnalytics;
using Portfolio.Api.Middleware;
using Portfolio.Application.TestAnalytics;
using Portfolio.Infrastructure.Persistence.Seed;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();
builder.Services.AddApiFoundation(builder.Configuration);
builder.Services.AddAdminAuthentication(builder.Configuration, builder.Environment);

var app = builder.Build();
var mediaRoot = Path.GetFullPath(builder.Configuration["Media:LocalPath"] ?? Path.Combine(AppContext.BaseDirectory, "media"));
Directory.CreateDirectory(mediaRoot);
var seedDevelopment = args.Contains("--seed-development-projects", StringComparer.OrdinalIgnoreCase);
var seedPreview = args.Contains("--seed-preview-projects", StringComparer.OrdinalIgnoreCase);
var telemetryArgument = Array.FindIndex(args, value => value.Equals("--import-preview-test-telemetry", StringComparison.OrdinalIgnoreCase));
if (seedDevelopment || seedPreview)
{
    if (!(seedDevelopment && app.Environment.IsDevelopment()) && !(seedPreview && app.Environment.IsEnvironment("Preview")))
        throw new InvalidOperationException("Project seed command does not match the current environment.");
    var connectionString = builder.Configuration.GetConnectionString("PortfolioDatabase")
        ?? throw new InvalidOperationException("Connection string 'PortfolioDatabase' is required.");
    var allowRemoteDatabase = seedPreview && app.Environment.IsEnvironment("Preview");
    var result = await DevelopmentProjectSeed.SeedAsync(app.Services, connectionString, allowRemoteDatabase);
    Console.WriteLine("Development project seed complete: {0} projects, {1} technologies, {2} relationships added.",
        result.ProjectsAdded, result.TechnologiesAdded, result.RelationshipsAdded);
    if (seedPreview)
    {
        var infographicResult = await DevelopmentInfographicSeed.SeedAsync(app.Services, connectionString, allowRemoteDatabase: true);
        Console.WriteLine("Preview infographic seed complete: {0} categories, {1} tags, {2} infographics, {3} relationships added.",
            infographicResult.CategoriesAdded, infographicResult.TagsAdded, infographicResult.InfographicsAdded, infographicResult.RelationshipsAdded);
    }
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
if (telemetryArgument >= 0)
{
    if (!app.Environment.IsEnvironment("Preview"))
        throw new InvalidOperationException("Preview test telemetry can only be imported in the Preview environment.");
    if (telemetryArgument + 1 >= args.Length || string.IsNullOrWhiteSpace(args[telemetryArgument + 1]))
        throw new ArgumentException("A normalized telemetry JSON path is required after --import-preview-test-telemetry.");

    var telemetryPath = Path.GetFullPath(args[telemetryArgument + 1]);
    var request = JsonSerializer.Deserialize<TestTelemetryImportRequest>(
        await File.ReadAllTextAsync(telemetryPath), new JsonSerializerOptions(JsonSerializerDefaults.Web))
        ?? throw new InvalidOperationException("Normalized test telemetry is empty or invalid.");
    if (string.IsNullOrWhiteSpace(request.ProviderRunId) || string.IsNullOrWhiteSpace(request.CommitSha))
        throw new InvalidOperationException("Normalized test telemetry requires a provider run ID and commit SHA.");

    await using var scope = app.Services.CreateAsyncScope();
    var result = await scope.ServiceProvider.GetRequiredService<ITestTelemetryImporter>().ImportAsync(request, CancellationToken.None);
    Console.WriteLine("Preview test telemetry import: {0} Run: {1}. Imported: {2}.", result.Message, result.TestRunId, result.Imported);
    return;
}
if (args.Contains("--bootstrap-admin", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
    await db.Database.MigrateAsync();
    var options = scope.ServiceProvider.GetRequiredService<IOptions<AdminBootstrapOptions>>().Value;
    var bootstrap = scope.ServiceProvider.GetRequiredService<IAdminBootstrapService>();
    var result = await bootstrap.BootstrapAsync(new AdminBootstrapRequest(
        options.Email, options.UserName, options.FullName, options.Password,
        options.GoogleSubject, options.GoogleEmail), CancellationToken.None);
    Console.WriteLine(
        "Admin bootstrap complete. User: {0}; created: {1}; role assigned: {2}; Google linked: {3}.",
        result.UserId, result.UserCreated, result.RoleAssigned, result.GoogleLinked);
}
app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseConfiguredCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseStaticFiles(new StaticFileOptions { FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(mediaRoot), RequestPath = "/media", ServeUnknownFileTypes = false });
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Preview"))
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
app.MapGet("/", () => Results.Ok(new { name = "Portfolio.Api", status = "ready" }))
    .ExcludeFromDescription();
app.MapGet("/api", () => Results.Ok(new { name = "Portfolio.Api", status = "ready" }))
    .WithName("ApiVerification");
app.MapAuthenticationEndpoints();
app.MapProjectEndpoints();
app.MapInfographicEndpoints();
app.MapMediaEndpoints();
app.MapTestAnalyticsEndpoints();
app.MapAssistantEndpoints();
app.Run();

public partial class Program;
