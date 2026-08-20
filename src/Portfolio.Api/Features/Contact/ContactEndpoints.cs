using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Features.Authentication;
using Portfolio.Application.Authentication;
using Portfolio.Application.Contact;

namespace Portfolio.Api.Features.Contact;

internal static class ContactEndpoints
{
    internal static IEndpointRouteBuilder MapContactEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Public submission endpoints
        var publicGroup = endpoints.MapGroup("/api/contact-messages").WithTags("Contact");

        publicGroup.MapPost("/", async (
            CreateContactMessageRequest request,
            IContactService contactService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var pageRoute = httpContext.Request.Headers["X-Page-Route"].FirstOrDefault();
            var referrer = httpContext.Request.Headers.Referer.FirstOrDefault();
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

            try
            {
                var response = await contactService.SubmitContactMessageAsync(
                    request,
                    pageRoute,
                    referrer,
                    ipAddress,
                    cancellationToken);

                return Results.Created($"/api/contact-messages/{response.Id}", response);
            }
            catch (ArgumentException ex)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error",
                    detail: ex.Message);
            }
        })
        .RequireRateLimiting("contact-submission")
        .WithName("SubmitContactMessage");

        // Alternate alias for public convenience
        endpoints.MapPost("/api/contact", async (
            CreateContactMessageRequest request,
            IContactService contactService,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var pageRoute = httpContext.Request.Headers["X-Page-Route"].FirstOrDefault();
            var referrer = httpContext.Request.Headers.Referer.FirstOrDefault();
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

            try
            {
                var response = await contactService.SubmitContactMessageAsync(
                    request,
                    pageRoute,
                    referrer,
                    ipAddress,
                    cancellationToken);

                return Results.Created($"/api/contact-messages/{response.Id}", response);
            }
            catch (ArgumentException ex)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Validation Error",
                    detail: ex.Message);
            }
        })
        .RequireRateLimiting("contact-submission")
        .WithTags("Contact")
        .ExcludeFromDescription();

        // Admin Management endpoints
        var adminGroup = endpoints.MapGroup("/api/admin/contact-messages").WithTags("Admin Contact Messages");
        adminGroup.RequireAuthorization(AdminAuthorization.Policy)
            .AddEndpointFilter<AntiforgeryEndpointFilter>();

        adminGroup.MapGet("/", async (
            [AsParameters] ContactMessageQuery query,
            IContactService contactService,
            CancellationToken cancellationToken) =>
            Results.Ok(await contactService.GetAdminMessagesAsync(query, cancellationToken)))
            .WithName("GetAdminContactMessages");

        adminGroup.MapGet("/unread-count", async (
            IContactService contactService,
            CancellationToken cancellationToken) =>
            Results.Ok(await contactService.GetUnreadCountAsync(cancellationToken)))
            .WithName("GetAdminContactUnreadCount");

        adminGroup.MapGet("/{id:guid}", async (
            Guid id,
            IContactService contactService,
            CancellationToken cancellationToken) =>
        {
            var message = await contactService.GetAdminMessageByIdAsync(id, cancellationToken);
            return message is null ? Results.NotFound() : Results.Ok(message);
        })
        .WithName("GetAdminContactMessageById");

        adminGroup.MapPatch("/{id:guid}/read", async (
            Guid id,
            HttpContext httpContext,
            IContactService contactService,
            CancellationToken cancellationToken) =>
        {
            Guid.TryParse(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminUserId);
            var updated = await contactService.MarkAsReadAsync(id, adminUserId == Guid.Empty ? null : adminUserId, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("MarkContactMessageAsRead");

        adminGroup.MapPost("/{id:guid}/read", async (
            Guid id,
            HttpContext httpContext,
            IContactService contactService,
            CancellationToken cancellationToken) =>
        {
            Guid.TryParse(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminUserId);
            var updated = await contactService.MarkAsReadAsync(id, adminUserId == Guid.Empty ? null : adminUserId, cancellationToken);
            return Results.Ok(updated);
        })
        .ExcludeFromDescription();

        adminGroup.MapPatch("/{id:guid}/unread", async (
            Guid id,
            HttpContext httpContext,
            IContactService contactService,
            CancellationToken cancellationToken) =>
        {
            Guid.TryParse(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminUserId);
            var updated = await contactService.MarkAsUnreadAsync(id, adminUserId == Guid.Empty ? null : adminUserId, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("MarkContactMessageAsUnread");

        adminGroup.MapPost("/{id:guid}/unread", async (
            Guid id,
            HttpContext httpContext,
            IContactService contactService,
            CancellationToken cancellationToken) =>
        {
            Guid.TryParse(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminUserId);
            var updated = await contactService.MarkAsUnreadAsync(id, adminUserId == Guid.Empty ? null : adminUserId, cancellationToken);
            return Results.Ok(updated);
        })
        .ExcludeFromDescription();

        adminGroup.MapPatch("/{id:guid}/archive", async (
            Guid id,
            HttpContext httpContext,
            IContactService contactService,
            CancellationToken cancellationToken) =>
        {
            Guid.TryParse(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminUserId);
            var updated = await contactService.ArchiveAsync(id, adminUserId == Guid.Empty ? null : adminUserId, cancellationToken);
            return Results.Ok(updated);
        })
        .WithName("ArchiveContactMessage");

        adminGroup.MapPost("/{id:guid}/archive", async (
            Guid id,
            HttpContext httpContext,
            IContactService contactService,
            CancellationToken cancellationToken) =>
        {
            Guid.TryParse(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), out var adminUserId);
            var updated = await contactService.ArchiveAsync(id, adminUserId == Guid.Empty ? null : adminUserId, cancellationToken);
            return Results.Ok(updated);
        })
        .ExcludeFromDescription();

        adminGroup.MapContactAnalyticsEndpoints();

        return endpoints;
    }
}
