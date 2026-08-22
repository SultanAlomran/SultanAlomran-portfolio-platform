using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.ContentInsights;
using Portfolio.Application.Infographics;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.IntegrationTests;

public sealed class ContentInsightsApiTests : IAsyncLifetime
{
    private readonly ContentInsightsApiFactory factory = new();
    private HttpClient client = null!;
    private HttpClient publicClient = null!;

    public async Task InitializeAsync()
    {
        await factory.InitializeDatabaseAsync();
        client = factory.CreateClient();
        await AuthenticationTestHelper.AuthenticateAsync(client);
        publicClient = factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        await factory.DeleteDatabaseAsync();
        await factory.DisposeAsync();
    }

    [Fact]
    public async Task Anonymous_view_tracking_records_view_and_sets_engagement_cookie()
    {
        using var response = await publicClient.PostAsync(
            $"/api/infographics/{factory.PublishedSlug}/view", new StringContent(""));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.True(response.Headers.Contains("Set-Cookie"));
        var cookieHeader = string.Join(";", response.Headers.GetValues("Set-Cookie"));
        Assert.Contains(".Portfolio.Engagement", cookieHeader);

        // Verify view recorded in database
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
        var viewsCount = await db.InfographicViews.CountAsync(x => x.InfographicId == factory.PublishedId);
        Assert.True(viewsCount >= 1);
    }

    [Fact]
    public async Task Anonymous_view_tracking_deduplicates_repeated_refreshes()
    {
        // First view
        using var firstResponse = await publicClient.PostAsync(
            $"/api/infographics/{factory.PublishedSlug}/view", new StringContent(""));
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Second view with same client session (cookie forwarded)
        using var secondResponse = await publicClient.PostAsync(
            $"/api/infographics/{factory.PublishedSlug}/view", new StringContent(""));
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
        var viewsCount = await db.InfographicViews.CountAsync(x => x.InfographicId == factory.PublishedId);
        // Due to deduplication within the 30-minute window, count should be 1
        Assert.Equal(1, viewsCount);
    }

    [Fact]
    public async Task Admin_content_insights_summary_requires_authentication()
    {
        using var unauthClient = factory.CreateClient();
        using var response = await unauthClient.GetAsync("/api/admin/content-insights/summary");
        Assert.True(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task Admin_content_insights_summary_returns_aggregated_metrics()
    {
        // Submit views, votes, and ratings first
        using var viewResponse = await publicClient.PostAsync(
            $"/api/infographics/{factory.PublishedSlug}/view", new StringContent(""));
        Assert.Equal(HttpStatusCode.OK, viewResponse.StatusCode);

        using var voteResponse = await publicClient.PutAsJsonAsync(
            $"/api/infographics/{factory.PublishedId}/helpful-vote",
            new SetHelpfulVoteRequest(true, null));
        Assert.Equal(HttpStatusCode.OK, voteResponse.StatusCode);

        using var ratingResponse = await publicClient.PutAsJsonAsync(
            $"/api/infographics/{factory.PublishedId}/rating",
            new SetInfographicRatingRequest(5));
        Assert.Equal(HttpStatusCode.OK, ratingResponse.StatusCode);

        // Query summary as authenticated admin
        var summary = await client.GetFromJsonAsync<ContentInsightsSummaryDto>("/api/admin/content-insights/summary?dateRange=30d");
        Assert.NotNull(summary);
        Assert.True(summary.TotalViews >= 1);
        Assert.True(summary.DeduplicatedViews >= 1);
        Assert.True(summary.HelpfulCount >= 1);
        Assert.Equal(100m, summary.HelpfulPercentage);
        Assert.True(summary.TotalRatings >= 1);
        Assert.Equal(5.00m, summary.AverageRating);
        Assert.True(summary.EngagementRate > 0m);
        Assert.NotEmpty(summary.RatingDistribution);
        Assert.NotEmpty(summary.Trend);
    }

    [Fact]
    public async Task Admin_content_insights_guides_returns_paged_results()
    {
        var result = await client.GetFromJsonAsync<InfographicPagedResult<InfographicInsightDto>>(
            "/api/admin/content-insights/guides?page=1&pageSize=10");

        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.Equal(1, result.Page);
        Assert.True(result.TotalCount >= 1);

        var guide = result.Items.FirstOrDefault(x => x.Id == factory.PublishedId);
        Assert.NotNull(guide);
        Assert.Equal(factory.PublishedSlug, guide.Slug);
        Assert.NotNull(guide.HealthStatus);
    }

    [Fact]
    public async Task Admin_content_insights_guide_drill_down_returns_specific_details()
    {
        var details = await client.GetFromJsonAsync<InfographicInsightDto>(
            $"/api/admin/content-insights/guides/{factory.PublishedId}?dateRange=30d");

        Assert.NotNull(details);
        Assert.Equal(factory.PublishedId, details.Id);
        Assert.Equal(factory.PublishedSlug, details.Slug);
        Assert.NotEmpty(details.RatingDistribution);
        Assert.NotEmpty(details.Trend);
    }
}

internal sealed class ContentInsightsApiFactory : WebApplicationFactory<Program>
{
    private readonly string connectionString = CreateConnectionString();
    public Guid CategoryId { get; private set; }
    public Guid PublishedId { get; private set; }
    public string PublishedSlug { get; private set; } = "insights-test-guide";

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:PortfolioDatabase", connectionString);

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
        await db.Database.MigrateAsync();
        await AuthenticationTestHelper.SeedAdministratorAsync(scope.ServiceProvider);

        var category = Category.Create("Cloud", "cloud", "Cloud infrastructure", 0);
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        CategoryId = category.Id;

        var guide = Infographic.Create(
            "Insights Test Guide", PublishedSlug, "Overview for insights testing",
            category.Id, DifficultyLevel.Intermediate);
        guide.Publish();
        db.Infographics.Add(guide);
        await db.SaveChangesAsync();
        PublishedId = guide.Id;
    }

    public async Task DeleteDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PortfolioDbContext>().Database.EnsureDeletedAsync();
    }

    private static string CreateConnectionString()
    {
        var databaseName = $"PortfolioContentInsightsTests_{Guid.NewGuid():N}";
        var configuredConnection = Environment.GetEnvironmentVariable("PORTFOLIO_TEST_SQL_CONNECTION_STRING");
        return string.IsNullOrWhiteSpace(configuredConnection)
            ? $"Server=(localdb)\\PortfolioPlatformLocal;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
            : $"{configuredConnection.TrimEnd(';')};Database={databaseName}";
    }
}
