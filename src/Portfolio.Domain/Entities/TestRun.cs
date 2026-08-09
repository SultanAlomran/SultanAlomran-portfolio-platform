using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class TestRun : AuditableEntity
{
    private TestRun() { }

    public static TestRun Create(TestTelemetryProvider provider, string providerRunId, string workflowName,
        long? workflowRunNumber, string branch, string commitSha, int? pullRequestNumber, string trigger,
        TestExecutionMode executionMode, TestRunStatus status, DateTime startedAtUtc, DateTime? completedAtUtc,
        long durationMs, int passedCount, int failedCount, int skippedCount, int flakyCount, int retryCount,
        string? browserSummary, string? featureSummary, string? repositoryUrl, string? workflowRunUrl,
        string? pullRequestUrl) => new()
        {
            Provider = provider,
            ProviderRunId = providerRunId.Trim(),
            WorkflowName = workflowName.Trim(),
            WorkflowRunNumber = workflowRunNumber,
            Branch = branch.Trim(),
            CommitSha = commitSha.Trim(),
            PullRequestNumber = pullRequestNumber,
            Trigger = trigger.Trim(),
            ExecutionMode = executionMode,
            Status = status,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc,
            DurationMs = durationMs,
            PassedCount = passedCount,
            FailedCount = failedCount,
            SkippedCount = skippedCount,
            FlakyCount = flakyCount,
            RetryCount = retryCount,
            BrowserSummary = browserSummary,
            FeatureSummary = featureSummary,
            RepositoryUrl = repositoryUrl,
            WorkflowRunUrl = workflowRunUrl,
            PullRequestUrl = pullRequestUrl
        };

    public TestTelemetryProvider Provider { get; private set; }
    public string ProviderRunId { get; private set; } = "";
    public string WorkflowName { get; private set; } = "";
    public long? WorkflowRunNumber { get; private set; }
    public string Branch { get; private set; } = "";
    public string CommitSha { get; private set; } = "";
    public int? PullRequestNumber { get; private set; }
    public string Trigger { get; private set; } = "";
    public TestExecutionMode ExecutionMode { get; private set; }
    public TestRunStatus Status { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public long DurationMs { get; private set; }
    public int PassedCount { get; private set; }
    public int FailedCount { get; private set; }
    public int SkippedCount { get; private set; }
    public int FlakyCount { get; private set; }
    public int RetryCount { get; private set; }
    public string? BrowserSummary { get; private set; }
    public string? FeatureSummary { get; private set; }
    public int TotalScreenshots { get; private set; }
    public int TotalVideos { get; private set; }
    public int TotalTraces { get; private set; }
    public int TotalReports { get; private set; }
    public string? RepositoryUrl { get; private set; }
    public string? WorkflowRunUrl { get; private set; }
    public string? PullRequestUrl { get; private set; }
    public ICollection<TestCaseResult> TestCaseResults { get; private set; } = new HashSet<TestCaseResult>();
    public ICollection<TestArtifact> Artifacts { get; private set; } = new HashSet<TestArtifact>();

    public void SetArtifactCounts(int screenshots, int videos, int traces, int reports)
    {
        TotalScreenshots = screenshots; TotalVideos = videos; TotalTraces = traces; TotalReports = reports;
    }
}
