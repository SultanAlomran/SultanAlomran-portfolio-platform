using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class TestCaseResult : Entity
{
    private TestCaseResult() { }
    public static TestCaseResult Create(string feature, string suite, string testName, string projectArea,
        string browser, string? viewport, TestCaseStatus status, long durationMs, int retryCount, bool isFlaky,
        string? errorType, string? errorSummary, string? sourceFile, DateTime? startedAtUtc, DateTime? completedAtUtc) => new()
        {
            Feature = feature.Trim(),
            Suite = suite.Trim(),
            TestName = testName.Trim(),
            ProjectArea = projectArea.Trim(),
            Browser = browser.Trim(),
            Viewport = viewport,
            Status = status,
            DurationMs = durationMs,
            RetryCount = retryCount,
            IsFlaky = isFlaky,
            ErrorType = errorType,
            ErrorSummary = errorSummary,
            SourceFile = sourceFile,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc
        };
    public Guid TestRunId { get; private set; }
    public string Feature { get; private set; } = "";
    public string Suite { get; private set; } = "";
    public string TestName { get; private set; } = "";
    public string ProjectArea { get; private set; } = "";
    public string Browser { get; private set; } = "";
    public string? Viewport { get; private set; }
    public TestCaseStatus Status { get; private set; }
    public long DurationMs { get; private set; }
    public int RetryCount { get; private set; }
    public bool IsFlaky { get; private set; }
    public string? ErrorType { get; private set; }
    public string? ErrorSummary { get; private set; }
    public string? SourceFile { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public TestRun TestRun { get; private set; } = null!;
    public ICollection<TestArtifact> Artifacts { get; private set; } = new HashSet<TestArtifact>();
}
