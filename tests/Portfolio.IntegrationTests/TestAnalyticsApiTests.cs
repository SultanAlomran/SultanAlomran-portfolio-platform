using System.Net;
using System.Net.Http.Json;
using Portfolio.Application.TestAnalytics;
using Portfolio.Domain.Enums;

namespace Portfolio.IntegrationTests;

public sealed class TestAnalyticsApiTests : IAsyncLifetime
{
    private readonly ProjectsApiFactory factory = new();
    private HttpClient client = null!;

    public async Task InitializeAsync() { await factory.InitializeDatabaseAsync(); client = factory.CreateClient(); }

    [Fact]
    public async Task Import_is_idempotent_and_dashboard_aggregates_real_results()
    {
        var request = Request("github-9001");
        using var first = await client.PostAsJsonAsync("/api/admin/test-analytics/import", request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        var imported = await first.Content.ReadFromJsonAsync<TestTelemetryImportResult>();
        Assert.True(imported!.Imported);

        using var duplicate = await client.PostAsJsonAsync("/api/admin/test-analytics/import", request);
        Assert.Equal(HttpStatusCode.OK, duplicate.StatusCode);
        Assert.False((await duplicate.Content.ReadFromJsonAsync<TestTelemetryImportResult>())!.Imported);

        using var overviewResponse = await client.GetAsync("/api/admin/test-analytics/overview?branch=feature/quality");
        var overviewBody = await overviewResponse.Content.ReadAsStringAsync();
        Assert.True(overviewResponse.IsSuccessStatusCode, overviewBody);
        var overview = await overviewResponse.Content.ReadFromJsonAsync<TestDashboardOverviewDto>();
        Assert.Equal(50, overview!.PassRate);
        Assert.Equal(1, overview.TotalRuns);
        Assert.Equal(3, overview.TestsExecuted);
        Assert.Equal(1, overview.FlakyTests);
        Assert.Equal(2, overview.BrowserCoverage);

        var runs = await client.GetFromJsonAsync<AnalyticsPagedResult<TestRunSummaryDto>>("/api/admin/test-analytics/runs?page=1&pageSize=1");
        Assert.Single(runs!.Items);
        var details = await client.GetFromJsonAsync<TestRunDetailsDto>($"/api/admin/test-analytics/runs/{runs.Items[0].Id}");
        Assert.Equal(3, details!.Tests.Count);
        Assert.Equal(TestArtifactAvailabilityStatus.Expired, details.Artifacts.Single().AvailabilityStatus);
    }

    [Fact]
    public async Task Filters_pagination_and_unknown_run_are_stable()
    {
        await client.PostAsJsonAsync("/api/admin/test-analytics/import", Request("run-one"));
        await client.PostAsJsonAsync("/api/admin/test-analytics/import", Request("run-two") with { Branch = "dev" });
        var filtered = await client.GetFromJsonAsync<AnalyticsPagedResult<TestRunSummaryDto>>("/api/admin/test-analytics/runs?branch=dev&page=1&pageSize=1");
        Assert.Single(filtered!.Items); Assert.Equal("dev", filtered.Items[0].Branch); Assert.Equal(1, filtered.TotalCount);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/admin/test-analytics/runs/{Guid.NewGuid()}")).StatusCode);
    }

    public async Task DisposeAsync() { client.Dispose(); await factory.DeleteDatabaseAsync(); await factory.DisposeAsync(); }

    private static TestTelemetryImportRequest Request(string providerRunId) => new(
        TestTelemetryProvider.GitHubActions, providerRunId, "Playwright E2E", 9001, "feature/quality", "abcdef123456",
        45, "pull_request", TestExecutionMode.Standard, TestRunStatus.Failed,
        DateTime.UtcNow.AddMinutes(-2), DateTime.UtcNow, "https://github.com/example/repo",
        "https://github.com/example/repo/actions/runs/9001", "https://github.com/example/repo/pull/45",
        [
            new("quality", "dashboard", "loads dashboard", "Admin", "chromium", "1440x900", TestCaseStatus.Passed, 1000, 0, false, null, null, "quality.spec.ts", null, null, "one"),
            new("quality", "dashboard", "loads details", "Admin", "chromium", "768x900", TestCaseStatus.Failed, 2000, 1, true, "AssertionError", "Expected heading to be visible", "quality.spec.ts", null, null, "two"),
            new("projects", "listing", "loads projects", "Public", "webkit", null, TestCaseStatus.Skipped, 0, 0, false, null, null, "projects.spec.ts", null, null, "three")
        ],
        [new("two", TestArtifactType.Trace, TestArtifactProvider.GitHubActions, "trace-artifact", "trace.zip",
            "application/zip", "https://github.com/example/repo/actions/runs/9001", "test-results/trace.zip", 1024,
            DateTime.UtcNow.AddDays(-20), DateTime.UtcNow.AddDays(-1), TestArtifactAvailabilityStatus.Available, "chromium", "quality")]);
}
