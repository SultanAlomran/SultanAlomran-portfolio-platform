using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Infographics;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;
using Portfolio.Infrastructure.Persistence.Seed;

namespace Portfolio.IntegrationTests;

public sealed class InfographicsApiTests : IAsyncLifetime
{
    private readonly InfographicsApiFactory factory = new();
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        await factory.InitializeDatabaseAsync();
        client = factory.CreateClient();
        await AuthenticationTestHelper.AuthenticateAsync(client);
    }

    [Fact]
    public async Task Complete_infographic_lifecycle_enforces_visibility_filters_and_ordering()
    {
        var request = Request("ef-core-visual-guide", factory.CategoryId, factory.TagId, factory.ImageId, factory.PdfId);
        using var createdResponse = await client.PostAsJsonAsync("/api/admin/infographics", request);
        Assert.Equal(HttpStatusCode.Created, createdResponse.StatusCode);
        var created = await createdResponse.Content.ReadFromJsonAsync<AdminInfographicDetailsDto>();
        Assert.NotNull(created);
        Assert.Equal(ContentStatus.Draft, created.Status);
        Assert.Equal([0, 1], created.Steps.Select(x => x.DisplayOrder).ToArray());
        Assert.Single(created.Tags);

        var publicBeforePublish = await client.GetFromJsonAsync<InfographicPagedResult<InfographicListItemDto>>("/api/infographics");
        Assert.Empty(publicBeforePublish!.Items);
        using var duplicate = await client.PostAsJsonAsync("/api/admin/infographics", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);

        using var publish = await client.PostAsync($"/api/admin/infographics/{created.Id}/publish", null);
        Assert.Equal(HttpStatusCode.OK, publish.StatusCode);
        var publicDetails = await client.GetFromJsonAsync<InfographicDetailsDto>("/api/infographics/ef-core-visual-guide");
        Assert.NotNull(publicDetails);
        Assert.Equal(2, publicDetails.Steps.Count);
        Assert.Equal("/media/guide.pdf", publicDetails.PdfUrl);

        var filtered = await client.GetFromJsonAsync<InfographicPagedResult<InfographicListItemDto>>(
            "/api/infographics?category=dotnet&tag=ef-core&difficulty=2&featured=true&page=1&pageSize=1");
        Assert.Single(filtered!.Items);
        Assert.Equal(1, filtered.TotalCount);

        using var archive = await client.PostAsync($"/api/admin/infographics/{created.Id}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archive.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/infographics/ef-core-visual-guide")).StatusCode);
    }

    [Fact]
    public async Task Invalid_infographic_returns_stable_validation_problem()
    {
        var invalid = Request("INVALID SLUG", factory.CategoryId, factory.TagId, factory.ImageId, factory.PdfId)
            with
        { Title = "", ShortDescription = "", CategoryId = Guid.NewGuid() };
        using var response = await client.PostAsJsonAsync("/api/admin/infographics", invalid);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Development_seed_is_idempotent_and_public_queries_exclude_drafts()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
        var first = await DevelopmentInfographicSeed.SeedAsync(db);
        var second = await DevelopmentInfographicSeed.SeedAsync(db);
        Assert.Equal(8, first.InfographicsAdded);
        Assert.True(first.CategoriesAdded > 0);
        Assert.True(first.TagsAdded > 0);
        Assert.Equal(new DevelopmentInfographicSeed.Result(0, 0, 0, 0), second);
        Assert.Equal(8, await db.Infographics.CountAsync());
        Assert.Equal(3, await db.Infographics.CountAsync(x => x.Status == ContentStatus.Published && x.IsFeatured));
        Assert.Equal(await db.Tags.CountAsync(), await db.Tags.Select(x => x.Name.ToLower()).Distinct().CountAsync());

        var featured = await client.GetFromJsonAsync<IReadOnlyList<InfographicListItemDto>>("/api/infographics/featured?count=3");
        Assert.Equal(3, featured!.Count);
        Assert.DoesNotContain(featured, x => x.Title == "Background Services in .NET");
        var publicList = await client.GetFromJsonAsync<InfographicPagedResult<InfographicListItemDto>>("/api/infographics?pageSize=20");
        Assert.Equal(7, publicList!.TotalCount);
    }

    public async Task DisposeAsync()
    {
        client.Dispose();
        await factory.DeleteDatabaseAsync();
        await factory.DisposeAsync();
    }

    private static UpsertInfographicRequest Request(string slug, Guid categoryId, Guid tagId, Guid imageId, Guid pdfId) => new(
        "EF Core Visual Guide", slug, "A concise guide to efficient EF Core reads.",
        "Learn how projection, tracking, pagination, and query plans shape efficient data access.", categoryId,
        DifficultyLevel.Intermediate, true, imageId, imageId, pdfId, [tagId],
        [new(1, "Project only what you need", "Use DTO projection for bounded read models.", null, 0),
         new(2, "Keep lists bounded", "Apply stable sorting and server-side pagination.", null, 1)],
        [new("EF Core performance guidance", "https://learn.microsoft.com/ef/core/performance/", "Documentation", 0)],
        [new("Read projection", "csharp", "query.AsNoTracking().Select(x => new Dto(x.Id));", null, 0)]);
}

internal sealed class InfographicsApiFactory : WebApplicationFactory<Program>
{
    private readonly string connectionString = CreateConnectionString();
    public Guid CategoryId { get; private set; }
    public Guid TagId { get; private set; }
    public Guid ImageId { get; private set; }
    public Guid PdfId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:PortfolioDatabase", connectionString);

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
        await db.Database.MigrateAsync();
        await AuthenticationTestHelper.SeedAdministratorAsync(scope.ServiceProvider);
        var category = Category.Create(".NET", "dotnet", "Modern .NET engineering.", 0);
        var tag = Tag.Create("EF Core", "ef-core");
        var image = MediaFile.Create("guide.png", "guide.png", "/media/guide.png", "image/png", 1024, "local", "Guide preview");
        var pdf = MediaFile.Create("guide.pdf", "guide.pdf", "/media/guide.pdf", "application/pdf", 2048, "local", "Guide PDF");
        db.Categories.Add(category); db.Tags.Add(tag); db.MediaFiles.AddRange(image, pdf);
        await db.SaveChangesAsync();
        CategoryId = category.Id; TagId = tag.Id; ImageId = image.Id; PdfId = pdf.Id;
    }

    public async Task DeleteDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PortfolioDbContext>().Database.EnsureDeletedAsync();
    }

    private static string CreateConnectionString()
    {
        var databaseName = $"PortfolioInfographicsTests_{Guid.NewGuid():N}";
        var configuredConnection = Environment.GetEnvironmentVariable("PORTFOLIO_TEST_SQL_CONNECTION_STRING");
        return string.IsNullOrWhiteSpace(configuredConnection)
            ? $"Server=(localdb)\\PortfolioPlatformLocal;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
            : $"{configuredConnection.TrimEnd(';')};Database={databaseName}";
    }
}
