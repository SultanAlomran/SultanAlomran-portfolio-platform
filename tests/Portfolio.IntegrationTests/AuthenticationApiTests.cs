using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.IntegrationTests;

public sealed class AuthenticationApiTests : IAsyncLifetime
{
    private readonly AuthenticationApiFactory factory = new();
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        await factory.InitializeDatabaseAsync();
        client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Fact]
    public async Task Anonymous_admin_and_quality_artifacts_return_401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/admin/projects")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.GetAsync($"/api/admin/test-analytics/artifacts/{Guid.NewGuid()}/content")).StatusCode);
    }

    [Fact]
    public async Task Local_login_me_and_logout_use_the_authorized_cookie_session()
    {
        await AuthenticationTestHelper.AuthenticateAsync(client, rememberMe: true);
        var current = await client.GetFromJsonAsync<CurrentAdmin>("/api/auth/me");
        Assert.NotNull(current);
        Assert.Equal(AuthenticationTestHelper.Email, current.Email);
        Assert.Contains("Administrator", current.Roles);
        Assert.Contains("content.manage", current.Permissions);
        Assert.Equal("Local", current.Provider);

        using var logout = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task Invalid_credentials_are_generic_and_missing_csrf_is_rejected()
    {
        using var missingCsrf = await client.PostAsJsonAsync("/api/auth/login",
            new { email = AuthenticationTestHelper.Email, password = "wrong", rememberMe = false });
        Assert.Equal(HttpStatusCode.BadRequest, missingCsrf.StatusCode);

        await AuthenticationTestHelper.AddCsrfAsync(client);
        using var invalid = await client.PostAsJsonAsync("/api/auth/login",
            new { email = "unknown@portfolio.test", password = "wrong", rememberMe = false });
        Assert.Equal(HttpStatusCode.Unauthorized, invalid.StatusCode);
        var problem = await invalid.Content.ReadFromJsonAsync<ProblemResponse>();
        Assert.Equal("Invalid email or password.", problem?.Title);
    }

    [Fact]
    public async Task Linked_google_identity_signs_in_and_external_return_urls_are_rejected()
    {
        using var response = await client.GetAsync("/api/auth/google?returnUrl=https%3A%2F%2Fevil.example");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("http://localhost:4300/dashboard", response.Headers.Location?.ToString());
        var current = await client.GetFromJsonAsync<CurrentAdmin>("/api/auth/me");
        Assert.Equal("Google", current?.Provider);
    }

    [Fact]
    public async Task Unknown_google_identity_is_denied_without_creating_an_admin()
    {
        using var unknownFactory = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("Authentication:Google:TestSubject", "unknown-google-subject"));
        using var unknown = unknownFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var response = await unknown.GetAsync("/api/auth/google?returnUrl=%2Fprojects");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/login?error=not-authorized", response.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.Unauthorized, (await unknown.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task Inactive_linked_user_is_denied_for_local_and_google_login()
    {
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
            var user = await db.Users.SingleAsync(x => x.Email == AuthenticationTestHelper.Email);
            user.SetActive(false);
            await db.SaveChangesAsync();
        }

        await AuthenticationTestHelper.AddCsrfAsync(client);
        using var local = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = AuthenticationTestHelper.Email,
            password = AuthenticationTestHelper.Password,
            rememberMe = false
        });
        Assert.Equal(HttpStatusCode.Unauthorized, local.StatusCode);
        Assert.Equal("Invalid email or password.", (await local.Content.ReadFromJsonAsync<ProblemResponse>())?.Title);
        using var google = await client.GetAsync("/api/auth/google");
        Assert.Contains("/login?error=not-authorized", google.Headers.Location?.ToString());
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await factory.DeleteDatabaseAsync();
        await factory.DisposeAsync();
    }

    private sealed record CurrentAdmin(Guid Id, string FullName, string Email, string[] Roles, string[] Permissions, string Provider);
    private sealed record ProblemResponse(string? Title);
}

internal sealed class AuthenticationApiFactory : WebApplicationFactory<Program>
{
    private readonly string connectionString = CreateConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:PortfolioDatabase", connectionString);
        builder.UseSetting("Authentication:Google:Enabled", "true");
        builder.UseSetting("Authentication:Google:TestMode", "true");
        builder.UseSetting("Authentication:Google:TestSubject", AuthenticationTestHelper.GoogleSubject);
        builder.UseSetting("Authentication:AdminBaseUrl", "http://localhost:4300");
    }

    internal async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
        await db.Database.MigrateAsync();
        await AuthenticationTestHelper.SeedAdministratorAsync(scope.ServiceProvider, AuthenticationTestHelper.GoogleSubject);
    }

    internal async Task DeleteDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PortfolioDbContext>().Database.EnsureDeletedAsync();
    }

    private static string CreateConnectionString()
    {
        var databaseName = $"PortfolioAuthenticationTests_{Guid.NewGuid():N}";
        var configuredConnection = Environment.GetEnvironmentVariable("PORTFOLIO_TEST_SQL_CONNECTION_STRING");
        return string.IsNullOrWhiteSpace(configuredConnection)
            ? $"Server=(localdb)\\PortfolioPlatformLocal;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
            : $"{configuredConnection.TrimEnd(';')};Database={databaseName}";
    }
}
