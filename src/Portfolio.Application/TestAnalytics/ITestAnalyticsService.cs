namespace Portfolio.Application.TestAnalytics;

public interface ITestAnalyticsService
{
    Task<TestDashboardOverviewDto> GetOverviewAsync(TestAnalyticsQuery query, CancellationToken cancellationToken);
    Task<AnalyticsPagedResult<TestRunSummaryDto>> GetRunsAsync(TestAnalyticsQuery query, CancellationToken cancellationToken);
    Task<TestRunDetailsDto?> GetRunAsync(Guid id, CancellationToken cancellationToken);
    Task<AnalyticsPagedResult<TestCaseResultDto>> GetTestsAsync(TestAnalyticsQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyList<FlakyTestDto>> GetFlakyTestsAsync(TestAnalyticsQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyList<BrowserStatisticDto>> GetBrowserStatisticsAsync(TestAnalyticsQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyList<FeatureCoverageDto>> GetFeatureCoverageAsync(TestAnalyticsQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyList<TestTrendPointDto>> GetTrendsAsync(TestAnalyticsQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyList<TestArtifactDto>> GetArtifactsAsync(Guid runId, CancellationToken cancellationToken);
}

public interface ITestTelemetryImporter
{
    Task<TestTelemetryImportResult> ImportAsync(TestTelemetryImportRequest request, CancellationToken cancellationToken);
}
