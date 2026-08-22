using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Assistant;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.IntegrationTests;

public sealed class VisualHandbookAiApiTests : IAsyncLifetime
{
    private readonly VisualHandbookAiApiFactory factory = new();
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        await factory.InitializeDatabaseAsync();
        client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSummary_ReturnsOk_ForPublishedGuide()
    {
        var response = await client.PostAsync("/api/ai/guides/ef-core-performance/summary", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var summary = await response.Content.ReadFromJsonAsync<GuideAiSummaryResponse>();
        Assert.NotNull(summary);
        Assert.Equal("ef-core-performance", summary.GuideSlug);
        Assert.Equal("EF Core Performance Checklist", summary.Title);
        Assert.NotEmpty(summary.Summary);
        Assert.NotEmpty(summary.KeyTakeaways);
        Assert.NotEmpty(summary.CommonUses);
    }

    [Fact]
    public async Task GetSummary_ReturnsNotFound_ForUnpublishedOrMissingGuide()
    {
        var response = await client.PostAsync("/api/ai/guides/non-existent-guide/summary", null);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Assistant_WithGuideSlug_ReturnsContextualResponse()
    {
        var payload = new AssistantMessageRequest(
            "Explain the main idea of this guide",
            [],
            "ef-core-performance");

        var response = await client.PostAsJsonAsync("/api/assistant/messages", payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<AssistantMessageResponse>();
        Assert.NotNull(result);
        Assert.NotEmpty(result.Message);
        Assert.Contains(result.Sources, s => s.Route.Contains("ef-core-performance"));
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await factory.DeleteDatabaseAsync();
        await factory.DisposeAsync();
    }
}

internal sealed class VisualHandbookAiApiFactory : WebApplicationFactory<Program>
{
    private readonly string connectionString = CreateConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:PortfolioDatabase", connectionString);
        builder.UseSetting("AiAssistant:Enabled", "true");
        builder.UseSetting("AiAssistant:Provider", "Deterministic");
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
        await db.Database.MigrateAsync();

        var category = Category.Create(".NET", "dotnet", "Modern .NET engineering.", 0);
        var tag = Tag.Create("EF Core", "ef-core");
        var image = MediaFile.Create("guide.png", "guide.png", "/media/guide.png", "image/png", 1024, "local", "Guide preview");

        db.Categories.Add(category);
        db.Tags.Add(tag);
        db.MediaFiles.Add(image);
        await db.SaveChangesAsync();

        var guide = Infographic.Create(
            "EF Core Performance Checklist",
            "ef-core-performance",
            "Key optimization strategies for EF Core.",
            category.Id,
            DifficultyLevel.Intermediate);

        guide.UpdateContent(
            guide.Title,
            guide.Slug,
            guide.ShortDescription,
            "Full performance checklist details.",
            category.Id,
            DifficultyLevel.Intermediate,
            true,
            image.Id,
            image.Id,
            null);

        guide.Steps.Add(InfographicStep.Create(1, "Use AsNoTracking", "Disable change tracking.", null, 1));
        guide.CodeExamples.Add(InfographicCodeExample.Create("AsNoTracking", "csharp", "context.Blogs.AsNoTracking();", null, 1));
        guide.InfographicTags.Add(InfographicTag.Create(tag.Id));
        guide.Publish();

        db.Infographics.Add(guide);
        await db.SaveChangesAsync();
    }

    public async Task DeleteDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PortfolioDbContext>().Database.EnsureDeletedAsync();
    }

    private static string CreateConnectionString()
    {
        var databaseName = $"PortfolioVisualHandbookAiTests_{Guid.NewGuid():N}";
        var configuredConnection = Environment.GetEnvironmentVariable("PORTFOLIO_TEST_SQL_CONNECTION_STRING");
        return string.IsNullOrWhiteSpace(configuredConnection)
            ? $"Server=(localdb)\\PortfolioPlatformLocal;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
            : $"{configuredConnection.TrimEnd(';')};Database={databaseName}";
    }
}
