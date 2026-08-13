using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Assistant;

namespace Portfolio.Api.Features.Assistant;

internal static class AssistantEndpoints
{
    internal static IEndpointRouteBuilder MapAssistantEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/assistant/messages", async ([FromBody] AssistantMessageRequest request,
            IPortfolioAssistantService service, CancellationToken token) =>
        {
            try { return Results.Ok(await service.SendAsync(request, token)); }
            catch (ArgumentException exception) { return Results.ValidationProblem(new Dictionary<string, string[]> { ["message"] = [exception.Message] }); }
            catch (AssistantUnavailableException) { return Results.Problem("The Portfolio Assistant is not available in this environment.", statusCode: 503); }
            catch (AssistantProviderException) { return Results.Problem("The Portfolio Assistant could not complete the request. Please try again.", statusCode: 502); }
        }).WithTags("Portfolio Assistant").RequireRateLimiting("assistant")
          .Produces<AssistantMessageResponse>().ProducesValidationProblem().ProducesProblem(429).ProducesProblem(502).ProducesProblem(503);
        return endpoints;
    }
}
