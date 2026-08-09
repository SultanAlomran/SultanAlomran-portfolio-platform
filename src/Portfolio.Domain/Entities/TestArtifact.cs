using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class TestArtifact : Entity
{
    private TestArtifact() { }
    public static TestArtifact Create(TestArtifactType artifactType, TestArtifactProvider provider,
        string? providerArtifactId, string name, string? mimeType, string? externalUrl, string? storagePath,
        long? sizeBytes, DateTime createdAtUtc, DateTime? expiresAtUtc,
        TestArtifactAvailabilityStatus availabilityStatus, string? browser, string? feature) => new()
        {
            ArtifactType = artifactType,
            Provider = provider,
            ProviderArtifactId = providerArtifactId,
            Name = name.Trim(),
            MimeType = mimeType,
            ExternalUrl = externalUrl,
            StoragePath = storagePath,
            SizeBytes = sizeBytes,
            CreatedAtUtc = createdAtUtc,
            ExpiresAtUtc = expiresAtUtc,
            AvailabilityStatus = availabilityStatus,
            Browser = browser,
            Feature = feature
        };
    public Guid TestRunId { get; private set; }
    public Guid? TestCaseResultId { get; private set; }
    public TestArtifactType ArtifactType { get; private set; }
    public TestArtifactProvider Provider { get; private set; }
    public string? ProviderArtifactId { get; private set; }
    public string Name { get; private set; } = "";
    public string? MimeType { get; private set; }
    public string? ExternalUrl { get; private set; }
    public string? StoragePath { get; private set; }
    public long? SizeBytes { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public TestArtifactAvailabilityStatus AvailabilityStatus { get; private set; }
    public string? Browser { get; private set; }
    public string? Feature { get; private set; }
    public TestRun TestRun { get; private set; } = null!;
    public TestCaseResult? TestCaseResult { get; private set; }
}
