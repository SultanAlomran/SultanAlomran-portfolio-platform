using Portfolio.Domain.Enums;

namespace Portfolio.Application.TestAnalytics;

public sealed record TestAnalyticsQuery(DateTime? From = null, DateTime? To = null, string? Branch = null,
    TestRunStatus? Status = null, string? Browser = null, string? Feature = null,
    TestExecutionMode? ExecutionMode = null, string Sort = "newest", int Page = 1, int PageSize = 20);

public sealed record AnalyticsPagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => TotalCount == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public sealed record TestDashboardOverviewDto(double PassRate, int TotalRuns, int TestsExecuted, int FailedTests,
    int FlakyTests, long AverageDurationMs, TestRunSummaryDto? LatestRun, int BrowserCoverage,
    IReadOnlyList<TestTrendPointDto> RunTrend, IReadOnlyList<TestTrendPointDto> DurationTrend,
    IReadOnlyList<BrowserStatisticDto> Browsers, IReadOnlyList<ExecutionModeStatisticDto> ExecutionModes,
    IReadOnlyList<FeatureCoverageDto> Features, IReadOnlyList<FlakyTestDto> Flaky,
    IReadOnlyList<SlowTestDto> Slowest);

public sealed record TestRunSummaryDto(Guid Id, string ProviderRunId, TestRunStatus Status, string Branch,
    string CommitSha, int? PullRequestNumber, string Trigger, TestExecutionMode ExecutionMode,
    string BrowserSummary, int PassedCount, int FailedCount, int SkippedCount, int FlakyCount,
    long DurationMs, DateTime StartedAtUtc, int ArtifactCount, string? WorkflowRunUrl);

public sealed record TestRunDetailsDto(TestRunSummaryDto Run, string WorkflowName, long? WorkflowRunNumber,
    DateTime? CompletedAtUtc, string FeatureSummary, string? RepositoryUrl, string? PullRequestUrl,
    IReadOnlyList<TestCaseResultDto> Tests, IReadOnlyList<TestArtifactDto> Artifacts);

public sealed record TestCaseResultDto(Guid Id, string Feature, string Suite, string TestName, string ProjectArea,
    string Browser, string? Viewport, TestCaseStatus Status, long DurationMs, int RetryCount, bool IsFlaky,
    string? ErrorType, string? ErrorSummary, string? SourceFile);

public sealed record TestArtifactDto(Guid Id, Guid? TestCaseResultId, TestArtifactType ArtifactType,
    TestArtifactProvider Provider, string? ProviderArtifactId, string Name, string? MimeType,
    string? ExternalUrl, string? StoragePath, long? SizeBytes, DateTime CreatedAtUtc, DateTime? ExpiresAtUtc,
    TestArtifactAvailabilityStatus AvailabilityStatus, string? Browser, string? Feature);

public sealed record TestTrendPointDto(DateTime Date, int Passed, int Failed, int Flaky, long AverageDurationMs);
public sealed record BrowserStatisticDto(string Browser, int Tests, int Passed, int Failed, double PassRate,
    long AverageDurationMs, TestCaseStatus LatestStatus);
public sealed record ExecutionModeStatisticDto(TestExecutionMode Mode, int Runs, double PassRate,
    DateTime? LatestRunAtUtc, int Artifacts);
public sealed record FeatureCoverageDto(string Feature, int Tests, DateTime LastTestedAtUtc,
    TestCaseStatus LatestStatus, int BrowserCoverage, bool HasVisualEvidence, bool HasRecording);
public sealed record FlakyTestDto(string TestName, string Feature, string Browser, int Executions,
    int Failures, int Retries, double FlakyRate, DateTime? LastFailedAtUtc, DateTime? LastPassedAtUtc);
public sealed record SlowTestDto(string TestName, string Feature, string Browser, long AverageDurationMs,
    long LatestDurationMs);

public sealed record TestTelemetryImportRequest(TestTelemetryProvider Provider, string ProviderRunId,
    string WorkflowName, long? WorkflowRunNumber, string Branch, string CommitSha, int? PullRequestNumber,
    string Trigger, TestExecutionMode ExecutionMode, TestRunStatus Status, DateTime StartedAtUtc,
    DateTime? CompletedAtUtc, string? RepositoryUrl, string? WorkflowRunUrl, string? PullRequestUrl,
    IReadOnlyList<TestCaseImportItem> Tests, IReadOnlyList<TestArtifactImportItem> Artifacts);

public sealed record TestCaseImportItem(string Feature, string Suite, string TestName, string ProjectArea,
    string Browser, string? Viewport, TestCaseStatus Status, long DurationMs, int RetryCount, bool IsFlaky,
    string? ErrorType, string? ErrorSummary, string? SourceFile, DateTime? StartedAtUtc, DateTime? CompletedAtUtc,
    string? CorrelationKey = null);

public sealed record TestArtifactImportItem(string? TestCaseCorrelationKey, TestArtifactType ArtifactType,
    TestArtifactProvider Provider, string? ProviderArtifactId, string Name, string? MimeType,
    string? ExternalUrl, string? StoragePath, long? SizeBytes, DateTime CreatedAtUtc, DateTime? ExpiresAtUtc,
    TestArtifactAvailabilityStatus AvailabilityStatus, string? Browser, string? Feature);

public sealed record TestTelemetryImportResult(Guid TestRunId, bool Imported, string Message);
