using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.IntegrationTests;

internal static class AuthenticationTestHelper
{
    internal const string Email = "admin.e2e@portfolio.test";
    internal const string Password = "E2E-only-password!2026";
    internal const string GoogleSubject = "google-e2e-linked-subject";

    internal static async Task SeedAdministratorAsync(IServiceProvider services, string? googleSubject = null)
    {
        var db = services.GetRequiredService<PortfolioDbContext>();
        if (await db.Users.AnyAsync(x => x.Email == Email)) return;

        var user = User.Create("admin.e2e", Email, "pending-hash", "Portfolio Test Administrator");
        var hasher = services.GetRequiredService<IPasswordHasher<User>>();
        user.SetPasswordHash(hasher.HashPassword(user, Password));
        var role = await db.Roles.SingleAsync(x => x.Name == "Administrator");
        db.Users.Add(user);
        await db.SaveChangesAsync();
        db.UserRoles.Add(UserRole.Create(user.Id, role.Id));
        if (!string.IsNullOrWhiteSpace(googleSubject))
            db.UserExternalLogins.Add(UserExternalLogin.Create(user.Id, "Google", googleSubject, Email));
        await db.SaveChangesAsync();
    }

    internal static async Task AuthenticateAsync(HttpClient client, bool rememberMe = false)
    {
        var csrf = await client.GetFromJsonAsync<CsrfResponse>("/api/auth/csrf");
        Assert.NotNull(csrf);
        client.DefaultRequestHeaders.Remove(csrf.HeaderName);
        client.DefaultRequestHeaders.TryAddWithoutValidation(csrf.HeaderName, csrf.Token);
        using var response = await client.PostAsJsonAsync("/api/auth/login", new { email = Email, password = Password, rememberMe });
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, body);
        await AddCsrfAsync(client);
    }

    internal static async Task AddCsrfAsync(HttpClient client)
    {
        var csrf = await client.GetFromJsonAsync<CsrfResponse>("/api/auth/csrf");
        Assert.NotNull(csrf);
        client.DefaultRequestHeaders.Remove(csrf.HeaderName);
        client.DefaultRequestHeaders.TryAddWithoutValidation(csrf.HeaderName, csrf.Token);
    }

    private sealed record CsrfResponse(string Token, string HeaderName);
}
