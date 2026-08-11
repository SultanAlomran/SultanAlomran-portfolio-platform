using Microsoft.AspNetCore.Mvc;
using Portfolio.Application.Media;

namespace Portfolio.Api.Features.Media;

internal static class MediaEndpoints
{
    internal static IEndpointRouteBuilder MapMediaEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var media = endpoints.MapGroup("/api/admin/media").WithTags("Admin Media");
        media.MapGet("/", async ([AsParameters] MediaQuery query, IMediaService service, CancellationToken token) => Results.Ok(await service.ListAsync(query, token)));
        media.MapGet("/{id:guid}", async (Guid id, IMediaService service, CancellationToken token) => (await service.GetAsync(id, token)) is { } item ? Results.Ok(item) : Results.NotFound());
        media.MapPost("/", async ([FromForm] IFormFile file, IMediaService service, CancellationToken token) =>
        {
            await using var stream = file.OpenReadStream();
            var created = await service.UploadAsync(new(file.FileName, file.ContentType, file.Length, stream), token);
            return Results.Created($"/api/admin/media/{created.Id}", created);
        }).DisableAntiforgery().Accepts<IFormFile>("multipart/form-data");
        media.MapDelete("/{id:guid}", async (Guid id, IMediaService service, CancellationToken token) => { await service.DeleteAsync(id, token); return Results.NoContent(); });
        return endpoints;
    }
}
