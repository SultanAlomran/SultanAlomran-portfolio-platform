using Portfolio.Api.Configuration;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Portfolio.Application.Authentication;

namespace Portfolio.Api.Features.Authentication;

internal sealed class AdminCookieAuthenticationEvents(IAdminAuthenticationService authenticationService)
    : CookieAuthenticationEvents
{
    private const string LastValidatedKey = "admin:last-validated";

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var now = DateTimeOffset.UtcNow;
        if (context.Properties.Items.TryGetValue(LastValidatedKey, out var value)
            && long.TryParse(value, CultureInfo.InvariantCulture, out var ticks)
            && now - new DateTimeOffset(ticks, TimeSpan.Zero) < TimeSpan.FromMinutes(5))
            return;

        var identifier = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(identifier, out var userId))
        {
            await RejectAsync(context);
            return;
        }

        var user = await authenticationService.GetCurrentAsync(userId, context.HttpContext.RequestAborted);
        if (user is null)
        {
            await RejectAsync(context);
            return;
        }

        var provider = context.Principal?.FindFirstValue(AdminAuthorization.ProviderClaim) ?? "Local";
        context.ReplacePrincipal(AdminClaimsPrincipalFactory.Create(user, provider));
        context.Properties.Items[LastValidatedKey] = now.UtcTicks.ToString(CultureInfo.InvariantCulture);
        context.ShouldRenew = true;
    }

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }

    private static async Task RejectAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(AdminAuthenticationSchemes.ApplicationCookie);
    }
}
