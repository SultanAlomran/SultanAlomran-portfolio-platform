using Microsoft.EntityFrameworkCore;
using Portfolio.Application.TestAnalytics;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.TestAnalytics;

internal sealed class TestAnalyticsService(PortfolioDbContext db) : ITestAnalyticsService, ITestTelemetryImporter
{
    public async Task<TestDashboardOverviewDto> GetOverviewAsync(TestAnalyticsQuery request, CancellationToken token)
    {
        var runs = ApplyRunFilters(db.TestRuns.AsNoTracking(), request);
        var totalRuns = await runs.CountAsync(token);
        var totals = await runs.GroupBy(_ => 1).Select(g => new
        {
            Tests = g.Sum(x => x.PassedCount + x.FailedCount + x.SkippedCount),
            Passed = g.Sum(x => x.PassedCount),
            Failed = g.Sum(x => x.FailedCount),
            Flaky = g.Sum(x => x.FlakyCount),
            AverageDuration = (long?)g.Average(x => x.DurationMs)
        }).SingleOrDefaultAsync(token);
        var latest = await ProjectRuns(runs.OrderByDescending(x => x.StartedAtUtc)).FirstOrDefaultAsync(token);
        var browsers = await GetBrowserStatisticsAsync(request, token);
        var modeAggregates = await runs.GroupBy(x => x.ExecutionMode).Select(g => new
        {
            Mode = g.Key,
            Runs = g.Count(),
            Passed = g.Sum(x => x.PassedCount),
            Decided = g.Sum(x => x.PassedCount + x.FailedCount),
            LatestRunAtUtc = g.Max(x => (DateTime?)x.StartedAtUtc),
            Artifacts = g.Sum(x => x.TotalScreenshots + x.TotalVideos + x.TotalTraces + x.TotalReports)
        }).OrderBy(x => x.Mode).ToListAsync(token);
        var modes = modeAggregates.Select(x => new ExecutionModeStatisticDto(x.Mode, x.Runs,
            x.Decided == 0 ? 0 : Math.Round(x.Passed * 100.0 / x.Decided, 1), x.LatestRunAtUtc, x.Artifacts)).ToList();
        var trends = await GetTrendsAsync(request, token);
        var features = await GetFeatureCoverageAsync(request, token);
        var flaky = await GetFlakyTestsAsync(request, token);
        var slowQuery = ApplyCaseFilters(db.TestCaseResults.AsNoTracking(), request);
        var slowestData = await slowQuery
            .GroupBy(x => new { x.TestName, x.Feature, x.Browser })
            .Select(g => new
            {
                g.Key.TestName,
                g.Key.Feature,
                g.Key.Browser,
                AverageDurationMs = g.Average(x => x.DurationMs)
            })
            .OrderByDescending(x => x.AverageDurationMs).Take(10).ToListAsync(token);
        var slowLatestDates = slowQuery.GroupBy(x => new { x.TestName, x.Feature, x.Browser }).Select(g => new
        {
            g.Key.TestName,
            g.Key.Feature,
            g.Key.Browser,
            StartedAtUtc = g.Max(x => x.TestRun.StartedAtUtc)
        });
        var slowLatest = await (from item in slowQuery
                                join date in slowLatestDates
            on new { item.TestName, item.Feature, item.Browser, StartedAtUtc = item.TestRun.StartedAtUtc }
            equals new { date.TestName, date.Feature, date.Browser, date.StartedAtUtc }
                                select new { item.TestName, item.Feature, item.Browser, item.DurationMs }).ToListAsync(token);
        var slowest = slowestData.Select(x => new SlowTestDto(x.TestName, x.Feature, x.Browser,
            (long)x.AverageDurationMs, slowLatest.First(y => y.TestName == x.TestName && y.Feature == x.Feature && y.Browser == x.Browser).DurationMs)).ToList();
        var executed = totals?.Tests ?? 0;
        var decided = (totals?.Passed ?? 0) + (totals?.Failed ?? 0);
        return new(decided == 0 ? 0 : Math.Round((totals!.Passed * 100.0) / decided, 1), totalRuns, executed,
            totals?.Failed ?? 0, totals?.Flaky ?? 0, totals?.AverageDuration ?? 0, latest, browsers.Count,
            trends, trends, browsers, modes, features, flaky, slowest);
    }

    public async Task<AnalyticsPagedResult<TestRunSummaryDto>> GetRunsAsync(TestAnalyticsQuery request, CancellationToken token)
    {
        var query = ApplyRunFilters(db.TestRuns.AsNoTracking(), request);
        var total = await query.CountAsync(token); var page = Math.Max(1, request.Page); var size = Math.Clamp(request.PageSize, 1, 100);
        var sorted = request.Sort.ToLowerInvariant() switch
        {
            "oldest" => query.OrderBy(x => x.StartedAtUtc).ThenBy(x => x.Id),
            "duration" => query.OrderByDescending(x => x.DurationMs).ThenByDescending(x => x.StartedAtUtc),
            _ => query.OrderByDescending(x => x.StartedAtUtc).ThenByDescending(x => x.Id)
        };
        return new(await ProjectRuns(sorted).Skip((page - 1) * size).Take(size).ToListAsync(token), page, size, total);
    }

    public async Task<TestRunDetailsDto?> GetRunAsync(Guid id, CancellationToken token)
    {
        var run = await db.TestRuns.AsNoTracking().Where(x => x.Id == id).Select(x => new
        {
            Summary = new TestRunSummaryDto(x.Id, x.ProviderRunId, x.Status, x.Branch, x.CommitSha,
                x.PullRequestNumber, x.Trigger, x.ExecutionMode, x.BrowserSummary ?? "", x.PassedCount,
                x.FailedCount, x.SkippedCount, x.FlakyCount, x.DurationMs, x.StartedAtUtc, x.Artifacts.Count,
                x.WorkflowRunUrl),
            x.WorkflowName,
            x.WorkflowRunNumber,
            x.CompletedAtUtc,
            FeatureSummary = x.FeatureSummary ?? "",
            x.RepositoryUrl,
            x.PullRequestUrl
        }).SingleOrDefaultAsync(token);
        if (run is null) return null;
        var tests = await db.TestCaseResults.AsNoTracking().Where(x => x.TestRunId == id)
            .OrderBy(x => x.Status).ThenByDescending(x => x.DurationMs).Select(ProjectCase()).ToListAsync(token);
        var artifacts = await GetArtifactsAsync(id, token);
        return new(run.Summary, run.WorkflowName, run.WorkflowRunNumber, run.CompletedAtUtc, run.FeatureSummary,
            run.RepositoryUrl, run.PullRequestUrl, tests, artifacts);
    }

    public async Task<AnalyticsPagedResult<TestCaseResultDto>> GetTestsAsync(TestAnalyticsQuery request, CancellationToken token)
    {
        var query = ApplyCaseFilters(db.TestCaseResults.AsNoTracking(), request);
        var total = await query.CountAsync(token); var page = Math.Max(1, request.Page); var size = Math.Clamp(request.PageSize, 1, 100);
        var items = await query.OrderByDescending(x => x.TestRun.StartedAtUtc).ThenBy(x => x.TestName)
            .Skip((page - 1) * size).Take(size).Select(ProjectCase()).ToListAsync(token);
        return new(items, page, size, total);
    }

    public async Task<IReadOnlyList<FlakyTestDto>> GetFlakyTestsAsync(TestAnalyticsQuery request, CancellationToken token)
    {
        var data = await ApplyCaseFilters(db.TestCaseResults.AsNoTracking(), request).Where(x => x.IsFlaky || x.RetryCount > 0)
            .GroupBy(x => new { x.TestName, x.Feature, x.Browser })
            .Select(g => new
            {
                g.Key.TestName,
                g.Key.Feature,
                g.Key.Browser,
                Executions = g.Count(),
                Failures = g.Count(x => x.Status == TestCaseStatus.Failed),
                Retries = g.Sum(x => x.RetryCount),
                FlakyExecutions = g.Count(x => x.IsFlaky),
                LastFailedAtUtc = g.Where(x => x.Status == TestCaseStatus.Failed).Max(x => (DateTime?)x.TestRun.StartedAtUtc),
                LastPassedAtUtc = g.Where(x => x.Status == TestCaseStatus.Passed).Max(x => (DateTime?)x.TestRun.StartedAtUtc)
            })
            .OrderByDescending(x => x.FlakyExecutions).ThenByDescending(x => x.Retries).Take(20).ToListAsync(token);
        return data.Select(x => new FlakyTestDto(x.TestName, x.Feature, x.Browser, x.Executions, x.Failures,
            x.Retries, Math.Round(x.FlakyExecutions * 100.0 / x.Executions, 1), x.LastFailedAtUtc, x.LastPassedAtUtc)).ToList();
    }

    public async Task<IReadOnlyList<BrowserStatisticDto>> GetBrowserStatisticsAsync(TestAnalyticsQuery request, CancellationToken token)
    {
        var query = ApplyCaseFilters(db.TestCaseResults.AsNoTracking(), request);
        var statistics = await query.GroupBy(x => x.Browser).Select(g => new
        {
            Browser = g.Key,
            Tests = g.Count(),
            Passed = g.Count(x => x.Status == TestCaseStatus.Passed),
            Failed = g.Count(x => x.Status == TestCaseStatus.Failed),
            Decided = g.Count(x => x.Status == TestCaseStatus.Passed || x.Status == TestCaseStatus.Failed),
            AverageDurationMs = (long)g.Average(x => x.DurationMs)
        }).OrderBy(x => x.Browser).ToListAsync(token);
        var latestDates = query.GroupBy(x => x.Browser).Select(g => new { Browser = g.Key, StartedAtUtc = g.Max(x => x.TestRun.StartedAtUtc) });
        var latest = await (from item in query
                            join date in latestDates
            on new { item.Browser, StartedAtUtc = item.TestRun.StartedAtUtc } equals new { date.Browser, date.StartedAtUtc }
                            select new { item.Browser, item.Status }).ToListAsync(token);
        return statistics.Select(x => new BrowserStatisticDto(x.Browser, x.Tests, x.Passed, x.Failed,
            x.Decided == 0 ? 0 : Math.Round(x.Passed * 100.0 / x.Decided, 1), x.AverageDurationMs,
            latest.First(y => y.Browser == x.Browser).Status)).ToList();
    }

    public async Task<IReadOnlyList<FeatureCoverageDto>> GetFeatureCoverageAsync(TestAnalyticsQuery request, CancellationToken token)
    {
        var query = ApplyCaseFilters(db.TestCaseResults.AsNoTracking(), request);
        var statistics = await query.GroupBy(x => x.Feature).Select(g => new
        {
            Feature = g.Key,
            Tests = g.Count(),
            LastTestedAtUtc = g.Max(x => x.TestRun.StartedAtUtc),
            BrowserCoverage = g.Select(x => x.Browser).Distinct().Count(),
            HasVisualEvidence = g.Any(x => x.TestRun.ExecutionMode == TestExecutionMode.Visual),
            HasRecording = g.Any(x => x.TestRun.ExecutionMode == TestExecutionMode.FullRecording)
        }).OrderByDescending(x => x.LastTestedAtUtc).ToListAsync(token);
        var latestDates = query.GroupBy(x => x.Feature).Select(g => new { Feature = g.Key, StartedAtUtc = g.Max(x => x.TestRun.StartedAtUtc) });
        var latest = await (from item in query
                            join date in latestDates
            on new { item.Feature, StartedAtUtc = item.TestRun.StartedAtUtc } equals new { date.Feature, date.StartedAtUtc }
                            select new { item.Feature, item.Status }).ToListAsync(token);
        return statistics.Select(x => new FeatureCoverageDto(x.Feature, x.Tests, x.LastTestedAtUtc,
            latest.First(y => y.Feature == x.Feature).Status, x.BrowserCoverage, x.HasVisualEvidence, x.HasRecording)).ToList();
    }

    public async Task<IReadOnlyList<TestTrendPointDto>> GetTrendsAsync(TestAnalyticsQuery request, CancellationToken token)
    {
        var points = await ApplyRunFilters(db.TestRuns.AsNoTracking(), request).GroupBy(x => x.StartedAtUtc.Date)
            .Select(g => new
            {
                Date = g.Key,
                Passed = g.Sum(x => x.PassedCount),
                Failed = g.Sum(x => x.FailedCount),
                Flaky = g.Sum(x => x.FlakyCount),
                AverageDurationMs = g.Average(x => x.DurationMs)
            })
            .OrderBy(x => x.Date).Take(366).ToListAsync(token);
        return points.Select(x => new TestTrendPointDto(x.Date, x.Passed, x.Failed, x.Flaky, (long)x.AverageDurationMs)).ToList();
    }

    public async Task<IReadOnlyList<TestArtifactDto>> GetArtifactsAsync(Guid runId, CancellationToken token)
    {
        var now = DateTime.UtcNow;
        return await db.TestArtifacts.AsNoTracking().Where(x => x.TestRunId == runId).OrderBy(x => x.ArtifactType).ThenBy(x => x.Name)
            .Select(x => new TestArtifactDto(x.Id, x.TestCaseResultId, x.ArtifactType, x.Provider,
                x.ProviderArtifactId, x.Name, x.MimeType, x.ExternalUrl, x.StoragePath, x.SizeBytes,
                x.CreatedAtUtc, x.ExpiresAtUtc,
                x.AvailabilityStatus == TestArtifactAvailabilityStatus.Available && x.ExpiresAtUtc <= now
                    ? TestArtifactAvailabilityStatus.Expired : x.AvailabilityStatus,
                x.Browser, x.Feature)).ToListAsync(token);
    }

    public async Task<TestTelemetryImportResult> ImportAsync(TestTelemetryImportRequest request, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.ProviderRunId)) throw new ArgumentException("Provider run ID is required.");
        var existing = await db.TestRuns.AsNoTracking().Where(x => x.Provider == request.Provider && x.ProviderRunId == request.ProviderRunId)
            .Select(x => (Guid?)x.Id).SingleOrDefaultAsync(token);
        if (existing.HasValue) return new(existing.Value, false, "This provider run was already imported.");

        var passed = request.Tests.Count(x => x.Status == TestCaseStatus.Passed);
        var failed = request.Tests.Count(x => x.Status == TestCaseStatus.Failed);
        var skipped = request.Tests.Count(x => x.Status == TestCaseStatus.Skipped);
        var flaky = request.Tests.Count(x => x.IsFlaky);
        var retries = request.Tests.Sum(x => x.RetryCount);
        var duration = request.Tests.Sum(x => x.DurationMs);
        var run = TestRun.Create(request.Provider, request.ProviderRunId, request.WorkflowName,
            request.WorkflowRunNumber, request.Branch, request.CommitSha, request.PullRequestNumber, request.Trigger,
            request.ExecutionMode, request.Status, request.StartedAtUtc, request.CompletedAtUtc, duration,
            passed, failed, skipped, flaky, retries,
            string.Join(", ", request.Tests.Select(x => x.Browser).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()),
            string.Join(", ", request.Tests.Select(x => x.Feature).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct()),
            request.RepositoryUrl, request.WorkflowRunUrl, request.PullRequestUrl);
        var cases = new Dictionary<string, TestCaseResult>(StringComparer.Ordinal);
        foreach (var item in request.Tests)
        {
            var result = TestCaseResult.Create(item.Feature, item.Suite, item.TestName, item.ProjectArea,
                item.Browser, item.Viewport, item.Status, Math.Max(0, item.DurationMs), Math.Max(0, item.RetryCount),
                item.IsFlaky, item.ErrorType, Limit(item.ErrorSummary, 2000), item.SourceFile,
                item.StartedAtUtc, item.CompletedAtUtc);
            run.TestCaseResults.Add(result);
            if (!string.IsNullOrWhiteSpace(item.CorrelationKey)) cases[item.CorrelationKey] = result;
        }
        foreach (var item in request.Artifacts)
        {
            var artifact = TestArtifact.Create(item.ArtifactType, item.Provider, item.ProviderArtifactId,
                item.Name, item.MimeType, item.ExternalUrl, item.StoragePath, item.SizeBytes, item.CreatedAtUtc,
                item.ExpiresAtUtc, item.AvailabilityStatus, item.Browser, item.Feature);
            run.Artifacts.Add(artifact);
            if (item.TestCaseCorrelationKey is not null && cases.TryGetValue(item.TestCaseCorrelationKey, out var result))
                result.Artifacts.Add(artifact);
        }
        run.SetArtifactCounts(request.Artifacts.Count(x => x.ArtifactType == TestArtifactType.Screenshot),
            request.Artifacts.Count(x => x.ArtifactType == TestArtifactType.Video),
            request.Artifacts.Count(x => x.ArtifactType == TestArtifactType.Trace),
            request.Artifacts.Count(x => x.ArtifactType is TestArtifactType.HtmlReport or TestArtifactType.JUnit or TestArtifactType.Json));
        db.TestRuns.Add(run);
        await db.SaveChangesAsync(token);
        return new(run.Id, true, "Test telemetry imported.");
    }

    private static IQueryable<TestRun> ApplyRunFilters(IQueryable<TestRun> query, TestAnalyticsQuery request)
    {
        if (request.From.HasValue) query = query.Where(x => x.StartedAtUtc >= request.From.Value);
        if (request.To.HasValue) query = query.Where(x => x.StartedAtUtc < request.To.Value.AddDays(1));
        if (!string.IsNullOrWhiteSpace(request.Branch)) query = query.Where(x => x.Branch == request.Branch);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.ExecutionMode.HasValue) query = query.Where(x => x.ExecutionMode == request.ExecutionMode);
        if (!string.IsNullOrWhiteSpace(request.Browser)) query = query.Where(x => x.TestCaseResults.Any(t => t.Browser == request.Browser));
        if (!string.IsNullOrWhiteSpace(request.Feature)) query = query.Where(x => x.TestCaseResults.Any(t => t.Feature == request.Feature));
        return query;
    }

    private static IQueryable<TestCaseResult> ApplyCaseFilters(IQueryable<TestCaseResult> query, TestAnalyticsQuery request)
    {
        if (request.From.HasValue) query = query.Where(x => x.TestRun.StartedAtUtc >= request.From.Value);
        if (request.To.HasValue) query = query.Where(x => x.TestRun.StartedAtUtc < request.To.Value.AddDays(1));
        if (!string.IsNullOrWhiteSpace(request.Branch)) query = query.Where(x => x.TestRun.Branch == request.Branch);
        if (request.Status.HasValue) query = query.Where(x => x.TestRun.Status == request.Status);
        if (request.ExecutionMode.HasValue) query = query.Where(x => x.TestRun.ExecutionMode == request.ExecutionMode);
        if (!string.IsNullOrWhiteSpace(request.Browser)) query = query.Where(x => x.Browser == request.Browser);
        if (!string.IsNullOrWhiteSpace(request.Feature)) query = query.Where(x => x.Feature == request.Feature);
        return query;
    }

    private static IQueryable<TestRunSummaryDto> ProjectRuns(IQueryable<TestRun> query) => query.Select(x =>
        new TestRunSummaryDto(x.Id, x.ProviderRunId, x.Status, x.Branch, x.CommitSha, x.PullRequestNumber,
            x.Trigger, x.ExecutionMode, x.BrowserSummary ?? "", x.PassedCount, x.FailedCount, x.SkippedCount,
            x.FlakyCount, x.DurationMs, x.StartedAtUtc, x.Artifacts.Count, x.WorkflowRunUrl));

    private static System.Linq.Expressions.Expression<Func<TestCaseResult, TestCaseResultDto>> ProjectCase() => x =>
        new TestCaseResultDto(x.Id, x.Feature, x.Suite, x.TestName, x.ProjectArea, x.Browser, x.Viewport,
            x.Status, x.DurationMs, x.RetryCount, x.IsFlaky, x.ErrorType, x.ErrorSummary, x.SourceFile);

    private static string? Limit(string? value, int length) => string.IsNullOrWhiteSpace(value) ? null : value[..Math.Min(value.Length, length)];
}
