using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Contact;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.IntegrationTests;

public sealed class ContactApiTests : IAsyncLifetime
{
    private readonly ContactApiFactory factory = new();
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        await factory.InitializeDatabaseAsync();
        client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    public async Task DisposeAsync()
    {
        await factory.DeleteDatabaseAsync();
    }

    [Fact]
    public async Task Anonymous_visitor_can_submit_contact_message_successfully()
    {
        var request = new CreateContactMessageRequest(
            "Ahmed Alomran",
            "ahmed@example.com",
            "Senior .NET Opportunity",
            "Hello Sultan, I would like to discuss an opportunity.");

        using var response = await client.PostAsJsonAsync("/api/contact-messages", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PublicContactSubmissionResponse>();
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Contains("Sultan has been notified via Email and WhatsApp", result.Message);
    }

    [Fact]
    public async Task Public_contact_submission_validates_required_fields_and_email()
    {
        using var emptyName = await client.PostAsJsonAsync("/api/contact-messages",
            new CreateContactMessageRequest("", "valid@example.com", "Subject", "Message"));
        Assert.Equal(HttpStatusCode.BadRequest, emptyName.StatusCode);

        using var invalidEmail = await client.PostAsJsonAsync("/api/contact-messages",
            new CreateContactMessageRequest("Name", "not-an-email", "Subject", "Message"));
        Assert.Equal(HttpStatusCode.BadRequest, invalidEmail.StatusCode);

        using var emptySubject = await client.PostAsJsonAsync("/api/contact-messages",
            new CreateContactMessageRequest("Name", "valid@example.com", "", "Message"));
        Assert.Equal(HttpStatusCode.BadRequest, emptySubject.StatusCode);

        using var emptyMessage = await client.PostAsJsonAsync("/api/contact-messages",
            new CreateContactMessageRequest("Name", "valid@example.com", "Subject", ""));
        Assert.Equal(HttpStatusCode.BadRequest, emptyMessage.StatusCode);
    }

    [Fact]
    public async Task Anonymous_admin_access_returns_401_unauthorized()
    {
        var response = await client.GetAsync("/api/admin/contact-messages");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var unreadResponse = await client.GetAsync("/api/admin/contact-messages/unread-count");
        Assert.Equal(HttpStatusCode.Unauthorized, unreadResponse.StatusCode);
    }

    [Fact]
    public async Task Authenticated_admin_can_read_list_detail_and_manage_messages()
    {
        // 1. Submit a message anonymously
        var request = new CreateContactMessageRequest(
            "Sara Alharbi",
            "sara@example.com",
            "Project Discussion",
            "Could we schedule a call to discuss architecture?");
        var submitResponse = await client.PostAsJsonAsync("/api/contact-messages", request);
        var created = await submitResponse.Content.ReadFromJsonAsync<PublicContactSubmissionResponse>();
        Assert.NotNull(created);

        // 2. Authenticate admin
        await AuthenticationTestHelper.AuthenticateAsync(client);

        // 3. Get unread count
        var unreadResult = await client.GetFromJsonAsync<ContactUnreadCountDto>("/api/admin/contact-messages/unread-count");
        Assert.NotNull(unreadResult);
        Assert.True(unreadResult.UnreadCount >= 1);

        // 4. Get list
        var listResult = await client.GetFromJsonAsync<ContactPagedResult<ContactMessageSummaryDto>>("/api/admin/contact-messages");
        Assert.NotNull(listResult);
        Assert.Contains(listResult.Items, x => x.Id == created.Id);

        // 5. Get detail
        var detailResult = await client.GetFromJsonAsync<ContactMessageDto>($"/api/admin/contact-messages/{created.Id}");
        Assert.NotNull(detailResult);
        Assert.Equal("Sara Alharbi", detailResult.Name);
        Assert.Equal(ContactStatus.New, detailResult.Status);

        // 6. Mark read (with CSRF)
        await AuthenticationTestHelper.AddCsrfAsync(client);
        using var markRead = await client.PatchAsync($"/api/admin/contact-messages/{created.Id}/read", null);
        Assert.Equal(HttpStatusCode.OK, markRead.StatusCode);
        var readDto = await markRead.Content.ReadFromJsonAsync<ContactMessageDto>();
        Assert.Equal(ContactStatus.Read, readDto?.Status);

        // 7. Mark unread
        await AuthenticationTestHelper.AddCsrfAsync(client);
        using var markUnread = await client.PatchAsync($"/api/admin/contact-messages/{created.Id}/unread", null);
        Assert.Equal(HttpStatusCode.OK, markUnread.StatusCode);
        var unreadDto = await markUnread.Content.ReadFromJsonAsync<ContactMessageDto>();
        Assert.Equal(ContactStatus.New, unreadDto?.Status);

        // 8. Archive
        await AuthenticationTestHelper.AddCsrfAsync(client);
        using var archive = await client.PatchAsync($"/api/admin/contact-messages/{created.Id}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archive.StatusCode);
        var archivedDto = await archive.Content.ReadFromJsonAsync<ContactMessageDto>();
        Assert.Equal(ContactStatus.Archived, archivedDto?.Status);
    }

    [Fact]
    public async Task Admin_can_retrieve_real_analytics_metrics()
    {
        // 1. Create a message
        var request = new CreateContactMessageRequest(
            "Analytics Tester",
            "tester@example.com",
            "Analytics Subject",
            "Testing metrics aggregations.");
        await client.PostAsJsonAsync("/api/contact-messages", request);

        // 2. Authenticate
        await AuthenticationTestHelper.AuthenticateAsync(client);

        // 3. Get analytics
        var analytics = await client.GetFromJsonAsync<ContactAnalyticsDto>("/api/admin/contact-messages/analytics");
        Assert.NotNull(analytics);
        Assert.True(analytics.TotalMessages >= 1);
        Assert.True(analytics.NewMessages >= 1);
        Assert.Equal(30, analytics.Trend.Count);
        Assert.Contains(analytics.TopSubjects, s => s.Subject == "Analytics Subject");
    }

    [Fact]
    public async Task Admin_can_get_and_update_notification_settings()
    {
        // 1. Authenticate
        await AuthenticationTestHelper.AuthenticateAsync(client);
        await AuthenticationTestHelper.AddCsrfAsync(client);

        // 2. Get current settings
        var settings = await client.GetFromJsonAsync<Portfolio.Application.Notifications.NotificationSettingsDto>("/api/admin/settings/notifications");
        Assert.NotNull(settings);
        Assert.Equal("Deterministic", settings.EmailProvider);
        Assert.Equal("Deterministic", settings.WhatsAppProvider);

        // 3. Update settings
        var updateRequest = new Portfolio.Application.Notifications.UpdateNotificationSettingsRequest(
            EmailEnabled: false,
            WhatsAppEnabled: true,
            AdminToastEnabled: true);
        using var updateResponse = await client.PutAsJsonAsync("/api/admin/settings/notifications", updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<Portfolio.Application.Notifications.NotificationSettingsDto>();
        Assert.NotNull(updated);
        Assert.False(updated.EmailEnabled);
        Assert.True(updated.WhatsAppEnabled);
        Assert.True(updated.AdminToastEnabled);

        // 4. Verify roundtrip get
        var recheck = await client.GetFromJsonAsync<Portfolio.Application.Notifications.NotificationSettingsDto>("/api/admin/settings/notifications");
        Assert.NotNull(recheck);
        Assert.False(recheck.EmailEnabled);
    }
}

internal sealed class ContactApiFactory : WebApplicationFactory<Program>
{
    private readonly string connectionString = CreateConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:PortfolioDatabase", connectionString);
        builder.UseSetting("Notifications:Email:Provider", "Deterministic");
        builder.UseSetting("Notifications:WhatsApp:Provider", "Deterministic");
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
        await db.Database.MigrateAsync();
        await AuthenticationTestHelper.SeedAdministratorAsync(scope.ServiceProvider);
    }

    public async Task DeleteDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PortfolioDbContext>().Database.EnsureDeletedAsync();
    }

    private static string CreateConnectionString()
    {
        var databaseName = $"PortfolioContactTests_{Guid.NewGuid():N}";
        var configuredConnection = Environment.GetEnvironmentVariable("PORTFOLIO_TEST_SQL_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(configuredConnection))
        {
            return $"{configuredConnection.TrimEnd(';')};Database={databaseName}";
        }

        return $"Server=(localdb)\\PortfolioPlatformLocal;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True";
    }
}
