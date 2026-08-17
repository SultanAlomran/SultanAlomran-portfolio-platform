using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.TestAnalytics;

namespace Portfolio.Api.Features.TestAnalytics;

internal static class TestAnalyticsEndpoints
{
    internal static IEndpointRouteBuilder MapTestAnalyticsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/test-analytics").WithTags("Admin Test Analytics");
        group.MapGet("/overview", async ([AsParameters] TestAnalyticsQuery query, ITestAnalyticsService service, CancellationToken token) =>
            Results.Ok(await service.GetOverviewAsync(query, token)));
        group.MapGet("/runs", async ([AsParameters] TestAnalyticsQuery query, ITestAnalyticsService service, CancellationToken token) =>
            Results.Ok(await service.GetRunsAsync(query, token)));
        group.MapGet("/runs/{id:guid}", async (Guid id, ITestAnalyticsService service, CancellationToken token) =>
            await service.GetRunAsync(id, token) is { } run ? Results.Ok(run) : Results.NotFound());
        group.MapGet("/tests", async ([AsParameters] TestAnalyticsQuery query, ITestAnalyticsService service, CancellationToken token) =>
            Results.Ok(await service.GetTestsAsync(query, token)));
        group.MapGet("/flaky", async ([AsParameters] TestAnalyticsQuery query, ITestAnalyticsService service, CancellationToken token) =>
            Results.Ok(await service.GetFlakyTestsAsync(query, token)));
        group.MapGet("/browsers", async ([AsParameters] TestAnalyticsQuery query, ITestAnalyticsService service, CancellationToken token) =>
            Results.Ok(await service.GetBrowserStatisticsAsync(query, token)));
        group.MapGet("/features", async ([AsParameters] TestAnalyticsQuery query, ITestAnalyticsService service, CancellationToken token) =>
            Results.Ok(await service.GetFeatureCoverageAsync(query, token)));
        group.MapGet("/trends", async ([AsParameters] TestAnalyticsQuery query, ITestAnalyticsService service, CancellationToken token) =>
            Results.Ok(await service.GetTrendsAsync(query, token)));
        group.MapGet("/runs/{id:guid}/artifacts", async (Guid id, ITestAnalyticsService service, CancellationToken token) =>
            Results.Ok(await service.GetArtifactsAsync(id, token)));
        group.MapGet("/artifacts/{artifactId:guid}/content", async (Guid artifactId, bool? download,
            ITestArtifactContentService content, CancellationToken token) =>
        {
            var shouldDownload = download ?? false;
            var result = await content.ResolveAsync(artifactId, shouldDownload, token);
            return result.Status switch
            {
                TestArtifactContentStatus.Available => Results.File(result.PhysicalPath!, result.ContentType,
                    shouldDownload ? result.DownloadName : null, enableRangeProcessing: result.EnableRangeProcessing),
                TestArtifactContentStatus.NotFound => Results.NotFound(new { message = result.Message }),
                TestArtifactContentStatus.Gone => Results.Json(new { message = result.Message }, statusCode: StatusCodes.Status410Gone),
                TestArtifactContentStatus.Unsupported => Results.Json(new { message = result.Message }, statusCode: StatusCodes.Status415UnsupportedMediaType),
                _ => Results.Json(new { message = result.Message }, statusCode: StatusCodes.Status503ServiceUnavailable)
            };
        }).WithName("GetTestArtifactContent")
          .WithSummary("Preview or download a known Quality Dashboard artifact")
          .WithDescription("Resolves only artifact identifiers stored by Test Analytics. Preview MIME types are restricted; provider credentials remain server-side.")
          .Produces(StatusCodes.Status200OK)
          .Produces(StatusCodes.Status404NotFound)
          .Produces(StatusCodes.Status410Gone)
          .Produces(StatusCodes.Status415UnsupportedMediaType)
          .Produces(StatusCodes.Status503ServiceUnavailable);

        // Authentication is deferred platform-wide. Never expose ingestion in non-development environments.
        if (endpoints.ServiceProvider.GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            group.MapPost("/import", async (TestTelemetryImportRequest request, ITestTelemetryImporter importer, CancellationToken token) =>
            {
                if (string.IsNullOrWhiteSpace(request.ProviderRunId) || string.IsNullOrWhiteSpace(request.CommitSha))
                    return Results.ValidationProblem(new Dictionary<string, string[]> { ["run"] = ["Provider run ID and commit SHA are required."] });
                var result = await importer.ImportAsync(request, token);
                return result.Imported ? Results.Created($"/api/admin/test-analytics/runs/{result.TestRunId}", result) : Results.Ok(result);
            });
        }
        return endpoints;
    }
}
