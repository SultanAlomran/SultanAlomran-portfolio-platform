namespace Portfolio.Application.TestAnalytics;

public enum TestArtifactContentStatus
{
    Available,
    NotFound,
    Gone,
    Unsupported,
    Unavailable
}

public sealed record TestArtifactContentResult(
    TestArtifactContentStatus Status,
    string? PhysicalPath = null,
    string? ContentType = null,
    string? DownloadName = null,
    bool EnableRangeProcessing = false,
    string? Message = null);

public interface ITestArtifactContentService
{
    Task<TestArtifactContentResult> ResolveAsync(Guid artifactId, bool download, CancellationToken cancellationToken);
}
