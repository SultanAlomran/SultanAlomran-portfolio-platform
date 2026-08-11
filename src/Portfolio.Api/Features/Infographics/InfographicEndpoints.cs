using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Infographics;

namespace Portfolio.Api.Features.Infographics;

internal static class InfographicEndpoints
{
    internal static IEndpointRouteBuilder MapInfographicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var publicGroup = endpoints.MapGroup("/api/infographics").WithTags("Infographics");
        publicGroup.MapGet("/", async ([AsParameters] InfographicQuery query, IInfographicsService service, CancellationToken token) =>
            Results.Ok(await service.GetPublicAsync(query, token)));
        publicGroup.MapGet("/featured", async (int? count, IInfographicsService service, CancellationToken token) =>
            Results.Ok(await service.GetFeaturedAsync(count ?? 3, token)));
        publicGroup.MapGet("/{slug}", async (string slug, IInfographicsService service, CancellationToken token) =>
        {
            var item = await service.GetPublicBySlugAsync(slug, token);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });
        publicGroup.MapGet("/taxonomy/categories", async (IInfographicsService service, CancellationToken token) =>
            Results.Ok(await service.GetCategoriesAsync(token)));
        publicGroup.MapGet("/taxonomy/tags", async (IInfographicsService service, CancellationToken token) =>
            Results.Ok(await service.GetTagsAsync(token)));

        var admin = endpoints.MapGroup("/api/admin/infographics").WithTags("Admin Infographics");
        admin.MapGet("/", async ([AsParameters] InfographicQuery query, IInfographicsService service, CancellationToken token) =>
            Results.Ok(await service.GetAdminAsync(query, token)));
        admin.MapGet("/{id:guid}", async (Guid id, IInfographicsService service, CancellationToken token) =>
        {
            var item = await service.GetAdminByIdAsync(id, token);
            return item is null ? Results.NotFound() : Results.Ok(item);
        });
        admin.MapPost("/", async (UpsertInfographicRequest request, IInfographicsService service, CancellationToken token) =>
        {
            var item = await service.CreateAsync(request, token);
            return Results.Created($"/api/admin/infographics/{item.Id}", item);
        });
        admin.MapPut("/{id:guid}", async (Guid id, UpsertInfographicRequest request, IInfographicsService service, CancellationToken token) =>
            Results.Ok(await service.UpdateAsync(id, request, token)));
        admin.MapDelete("/{id:guid}", async (Guid id, IInfographicsService service, CancellationToken token) =>
        {
            await service.DeleteAsync(id, token);
            return Results.NoContent();
        });
        admin.MapPost("/{id:guid}/save-draft", async (Guid id, IInfographicsService service, CancellationToken token) =>
            Results.Ok(await service.SaveDraftAsync(id, token)));
        admin.MapGet("/{id:guid}/publish-readiness", async (Guid id, IInfographicsService service, CancellationToken token) =>
            Results.Ok(await service.GetPublishReadinessAsync(id, token)));
        admin.MapPost("/{id:guid}/publish", async (Guid id, IInfographicsService service, CancellationToken token) =>
            Results.Ok(await service.PublishAsync(id, token)));
        admin.MapPost("/{id:guid}/archive", async (Guid id, IInfographicsService service, CancellationToken token) =>
            Results.Ok(await service.ArchiveAsync(id, token)));
        admin.MapGet("/taxonomy/categories", async (IInfographicsService service, CancellationToken token) =>
            Results.Ok(await service.GetCategoriesAsync(token)));
        admin.MapGet("/taxonomy/tags", async (IInfographicsService service, CancellationToken token) =>
            Results.Ok(await service.GetTagsAsync(token)));
        admin.MapGet("/media", async (IInfographicsService service, CancellationToken token) =>
            Results.Ok(await service.GetMediaAsync(token)));
        return endpoints;
    }
}
