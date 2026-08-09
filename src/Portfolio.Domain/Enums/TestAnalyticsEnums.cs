namespace Portfolio.Domain.Enums;

public enum TestTelemetryProvider { Local = 0, GitHubActions = 1, External = 2 }
public enum TestExecutionMode { Standard = 0, Visual = 1, FullRecording = 2 }
public enum TestRunStatus { Running = 0, Passed = 1, Failed = 2, Cancelled = 3, TimedOut = 4 }
public enum TestCaseStatus { Passed = 0, Failed = 1, Skipped = 2, TimedOut = 3, Interrupted = 4 }
public enum TestArtifactType { HtmlReport = 0, Screenshot = 1, Video = 2, Trace = 3, JUnit = 4, Json = 5, Diagnostics = 6, Other = 7 }
public enum TestArtifactProvider { GitHubActions = 0, AzureBlob = 1, External = 2 }
public enum TestArtifactAvailabilityStatus { Available = 0, Expired = 1, Deleted = 2, Archived = 3 }
