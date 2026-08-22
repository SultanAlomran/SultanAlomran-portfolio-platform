using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Authentication;
using Portfolio.Application.ContentInsights;

namespace Portfolio.Api.Features.ContentInsights;

internal static class ContentInsightsEndpoints
{
    internal static IEndpointRouteBuilder MapContentInsightsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var admin = endpoints.MapGroup("/api/admin/content-insights").WithTags("Admin Content Insights");
        admin.RequireAuthorization(AdminAuthorization.Policy)
            .AddEndpointFilter<Authentication.AntiforgeryEndpointFilter>();

        admin.MapGet("/summary", async (
            [AsParameters] ContentInsightsFilter filter,
            IContentInsightsService service,
            CancellationToken token) =>
        {
            var summary = await service.GetSummaryAsync(filter, token);
            return Results.Ok(summary);
        });

        admin.MapGet("/guides", async (
            [AsParameters] ContentInsightsGuideQuery query,
            IContentInsightsService service,
            CancellationToken token) =>
        {
            var pagedGuides = await service.GetGuidesAsync(query, token);
            return Results.Ok(pagedGuides);
        });

        admin.MapGet("/guides/{id:guid}", async (
            Guid id,
            [AsParameters] ContentInsightsFilter filter,
            IContentInsightsService service,
            CancellationToken token) =>
        {
            var guide = await service.GetGuideDetailsAsync(id, filter, token);
            return guide is null ? Results.NotFound() : Results.Ok(guide);
        });

        return endpoints;
    }
}
