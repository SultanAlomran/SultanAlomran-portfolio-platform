using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Assistant;

namespace Portfolio.Api.Features.Assistant;

internal static class GuideAiEndpoints
{
    internal static IEndpointRouteBuilder MapGuideAiEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/ai/guides").WithTags("Visual Handbook AI");

        group.MapPost("/{slug}/summary", async (
            string slug,
            IGuideAiService service,
            CancellationToken token) =>
        {
            try
            {
                var summary = await service.GenerateSummaryAsync(slug, token);
                return Results.Ok(summary);
            }
            catch (ArgumentException exception)
            {
                return Results.ValidationProblem(new Dictionary<string, string[]> { ["slug"] = [exception.Message] });
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(new { error = $"Guide '{slug}' was not found or is unpublished." });
            }
            catch (AssistantUnavailableException)
            {
                return Results.Problem("AI summary is not available in this environment.", statusCode: 503);
            }
            catch (AssistantProviderException)
            {
                return Results.Problem("Could not generate AI summary. Please try again shortly.", statusCode: 502);
            }
        })
        .RequireRateLimiting("ai-summary")
        .Produces<GuideAiSummaryResponse>()
        .ProducesValidationProblem()
        .ProducesProblem(404)
        .ProducesProblem(429)
        .ProducesProblem(502)
        .ProducesProblem(503);

        return endpoints;
    }
}
