using Microsoft.AspNetCore.Mvc;
using Portfolio.Api.Features.Authentication;
using Portfolio.Application.Authentication;
using Portfolio.Application.Notifications;

namespace Portfolio.Api.Features.Notifications;

public static class NotificationSettingsEndpoints
{
    public static IEndpointRouteBuilder MapNotificationSettingsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin/settings").WithTags("Admin Settings");
        group.RequireAuthorization(AdminAuthorization.Policy)
            .AddEndpointFilter<AntiforgeryEndpointFilter>();

        group.MapGet("/notifications", GetSettingsAsync)
            .WithName("GetNotificationSettings")
            .WithSummary("Retrieve notification channels and configuration metadata.")
            .Produces<NotificationSettingsDto>(StatusCodes.Status200OK)
            .RequireAuthorization(AdminAuthorization.Policy);

        group.MapPut("/notifications", UpdateSettingsAsync)
            .WithName("UpdateNotificationSettings")
            .WithSummary("Update notification channel preferences.")
            .Produces<NotificationSettingsDto>(StatusCodes.Status200OK)
            .RequireAuthorization(AdminAuthorization.Policy);

        return endpoints;
    }

    private static async Task<IResult> GetSettingsAsync(
        [FromServices] INotificationSettingsService settingsService,
        CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetSettingsAsync(cancellationToken);
        return Results.Ok(settings);
    }

    private static async Task<IResult> UpdateSettingsAsync(
        [FromBody] UpdateNotificationSettingsRequest request,
        [FromServices] INotificationSettingsService settingsService,
        CancellationToken cancellationToken)
    {
        var updated = await settingsService.UpdateSettingsAsync(request, cancellationToken);
        return Results.Ok(updated);
    }
}
