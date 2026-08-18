using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Projects;

namespace Portfolio.Api.Features.Projects;

internal static class ProjectEndpoints
{
    internal static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var publicProjects = endpoints.MapGroup("/api/projects").WithTags("Projects");
        publicProjects.MapGet("/", async ([AsParameters] ProjectQuery query, IProjectsService service, CancellationToken token) =>
            Results.Ok(await service.GetPublicProjectsAsync(query, token)));
        publicProjects.MapGet("/{slug}", async (string slug, IProjectsService service, CancellationToken token) =>
        {
            var project = await service.GetPublicProjectBySlugAsync(slug, token);
            return project is null ? Results.NotFound() : Results.Ok(project);
        });
        endpoints.MapGet("/api/technologies", async (IProjectsService service, CancellationToken token) =>
            Results.Ok(await service.GetTechnologiesAsync(token))).WithTags("Projects");

        var admin = endpoints.MapGroup("/api/admin/projects").WithTags("Admin Projects");
        admin.RequireAuthorization(Portfolio.Application.Authentication.AdminAuthorization.Policy).AddEndpointFilter<Portfolio.Api.Features.Authentication.AntiforgeryEndpointFilter>();
        admin.MapGet("/", async ([AsParameters] ProjectQuery query, IProjectsService service, CancellationToken token) =>
            Results.Ok(await service.GetAdminProjectsAsync(query, token)));
        admin.MapGet("/{id:guid}", async (Guid id, IProjectsService service, CancellationToken token) =>
        {
            var project = await service.GetAdminProjectAsync(id, token);
            return project is null ? Results.NotFound() : Results.Ok(project);
        });
        admin.MapPost("/", async (UpsertProjectRequest request, IProjectsService service, CancellationToken token) =>
        {
            var project = await service.CreateAsync(request, token);
            return Results.Created($"/api/admin/projects/{project.Id}", project);
        });
        admin.MapPut("/{id:guid}", async (Guid id, UpsertProjectRequest request, IProjectsService service, CancellationToken token) =>
            Results.Ok(await service.UpdateAsync(id, request, token)));
        admin.MapDelete("/{id:guid}", async (Guid id, IProjectsService service, CancellationToken token) =>
        {
            await service.DeleteAsync(id, token);
            return Results.NoContent();
        });
        admin.MapPost("/{id:guid}/save-draft", async (Guid id, IProjectsService service, CancellationToken token) =>
            Results.Ok(await service.SaveDraftAsync(id, token)));
        admin.MapGet("/{id:guid}/publish-readiness", async (Guid id, IProjectsService service, CancellationToken token) =>
            Results.Ok(await service.GetPublishReadinessAsync(id, token)));
        admin.MapPost("/{id:guid}/publish", async (Guid id, IProjectsService service, CancellationToken token) =>
            Results.Ok(await service.PublishAsync(id, token)));
        admin.MapPost("/{id:guid}/archive", async (Guid id, IProjectsService service, CancellationToken token) =>
            Results.Ok(await service.ArchiveAsync(id, token)));
        admin.MapPost("/{id:guid}/feature", async (Guid id, IProjectsService service, CancellationToken token) =>
            Results.Ok(await service.SetFeaturedAsync(id, true, token)));
        admin.MapDelete("/{id:guid}/feature", async (Guid id, IProjectsService service, CancellationToken token) =>
            Results.Ok(await service.SetFeaturedAsync(id, false, token)));

        endpoints.MapGet("/api/admin/technologies", async (IProjectsService service, CancellationToken token) =>
            Results.Ok(await service.GetTechnologiesAsync(token))).WithTags("Admin Projects").RequireAuthorization(Portfolio.Application.Authentication.AdminAuthorization.Policy);
        return endpoints;
    }
}
