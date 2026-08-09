using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Projects;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.IntegrationTests;

public sealed class ProjectsApiTests : IAsyncLifetime
{
    private readonly ProjectsApiFactory factory = new();
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        await factory.InitializeDatabaseAsync();
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Complete_project_lifecycle_enforces_visibility_validation_and_relationship_ordering()
    {
        var request = Request("complete-project", factory.TechnologyId, factory.MediaFileId);
        using var createdResponse = await client.PostAsJsonAsync("/api/admin/projects", request);
        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        var created = await createdResponse.Content.ReadFromJsonAsync<AdminProjectDetailsDto>();
        Assert.NotNull(created);
        Assert.Equal(ContentStatus.Draft, created.Status);
        Assert.Single(created.Technologies);
        Assert.Equal(0, created.Images.Single().DisplayOrder);
        Assert.Equal(0, created.Links.Single().DisplayOrder);

        var publicBeforePublish = await client.GetFromJsonAsync<PagedResult<ProjectListItemDto>>("/api/projects");
        Assert.Empty(publicBeforePublish!.Items);

        using var duplicate = await client.PostAsJsonAsync("/api/admin/projects", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        using var publish = await client.PostAsync($"/api/admin/projects/{created.Id}/publish", null);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);
        var publicDetails = await client.GetFromJsonAsync<ProjectDetailsDto>("/api/projects/complete-project");
        Assert.Equal("Business problem", publicDetails!.BusinessProblem);

        var publicList = await client.GetFromJsonAsync<PagedResult<ProjectListItemDto>>("/api/projects?featured=true&page=1&pageSize=1");
        Assert.Single(publicList!.Items);
        Assert.Equal(1, publicList.TotalCount);

        using var archive = await client.PostAsync($"/api/admin/projects/{created.Id}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archive.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/projects/complete-project")).StatusCode);

        using var saveDraft = await client.PostAsync($"/api/admin/projects/{created.Id}/save-draft", null);
        Assert.Equal(HttpStatusCode.OK, saveDraft.StatusCode);
        using var update = await client.PutAsJsonAsync($"/api/admin/projects/{created.Id}", Request("updated-project", factory.TechnologyId, factory.MediaFileId));
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = await update.Content.ReadFromJsonAsync<AdminProjectDetailsDto>();
        Assert.Equal("updated-project", updated!.Slug);

        using var delete = await client.DeleteAsync($"/api/admin/projects/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/admin/projects/{created.Id}")).StatusCode);
    }

    [Fact]
    public async Task Invalid_project_returns_stable_validation_problem()
    {
        var invalid = Request("INVALID SLUG", factory.TechnologyId, factory.MediaFileId) with { Title = "", ShortDescription = "" };
        using var response = await client.PostAsJsonAsync("/api/admin/projects", invalid);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await factory.DeleteDatabaseAsync();
        await factory.DisposeAsync();
    }

    private static UpsertProjectRequest Request(string slug, Guid technologyId, Guid mediaFileId) => new(
        "Complete Project", slug, "A concise public summary.", "Overview", "Business problem", "Solution",
        "Architecture", "Feature one", "Challenge", "Measurable impact", "Lessons learned", mediaFileId,
        "https://example.com", true, [new(technologyId)], [new(mediaFileId, "Project screenshot", null, 0)],
        [new("Live project", "https://example.com", "live", 0)]);
}

internal sealed class ProjectsApiFactory : WebApplicationFactory<Program>
{
    private readonly string connectionString = CreateConnectionString();
    public Guid TechnologyId { get; private set; }
    public Guid MediaFileId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:PortfolioDatabase", connectionString);

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
        await db.Database.MigrateAsync();
        var technology = Technology.Create("Angular", "Frontend", "angular");
        var media = MediaFile.Create("project.png", "project.png", "/media/project.png", "image/png", 1024, "local", "Project preview");
        db.Technologies.Add(technology); db.MediaFiles.Add(media);
        await db.SaveChangesAsync();
        TechnologyId = technology.Id; MediaFileId = media.Id;
    }

    public async Task DeleteDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PortfolioDbContext>().Database.EnsureDeletedAsync();
    }

    private static string CreateConnectionString()
    {
        var databaseName = $"PortfolioProjectsTests_{Guid.NewGuid():N}";
        var configuredConnection = Environment.GetEnvironmentVariable("PORTFOLIO_TEST_SQL_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(configuredConnection))
        {
            return $"{configuredConnection.TrimEnd(';')};Database={databaseName}";
        }

        return $"Server=(localdb)\\PortfolioPlatformLocal;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True";
    }
}
