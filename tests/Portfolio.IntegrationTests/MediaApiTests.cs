using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Application.Media;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.IntegrationTests;

public sealed class MediaApiTests : IAsyncLifetime
{
    private readonly MediaApiFactory factory = new();
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        await factory.InitializeDatabaseAsync();
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Image_upload_list_detail_and_safe_delete_complete()
    {
        var created = await UploadAsync("test-cover.png", "image/png", PngBytes());
        Assert.Equal("image/png", created.ContentType);
        Assert.False(created.IsReferenced);

        var page = await client.GetFromJsonAsync<MediaPage>("/api/admin/media?type=image&usage=unreferenced");
        Assert.NotNull(page);
        Assert.Contains(page.Items, x => x.Id == created.Id);

        var detail = await client.GetFromJsonAsync<MediaFileDto>($"/api/admin/media/{created.Id}");
        Assert.Equal("test-cover.png", detail!.OriginalFileName);

        using var deleted = await client.DeleteAsync($"/api/admin/media/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
    }

    [Fact]
    public async Task Pdf_upload_is_supported()
    {
        var created = await UploadAsync("test-document.pdf", "application/pdf", "%PDF-1.4\n%%EOF"u8.ToArray());
        Assert.Equal("application/pdf", created.ContentType);
    }

    [Fact]
    public async Task Spoofed_or_unsupported_upload_is_rejected()
    {
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent("not an image"u8.ToArray()), "file", "unsafe.png");
        using var response = await client.PostAsync("/api/admin/media", form);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Referenced_media_cannot_be_deleted()
    {
        using var response = await client.DeleteAsync($"/api/admin/media/{factory.ReferencedMediaId}");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private async Task<MediaFileDto> UploadAsync(string name, string contentType, byte[] bytes)
    {
        using var form = new MultipartFormDataContent();
        var content = new ByteArrayContent(bytes);
        content.Headers.ContentType = new(contentType);
        form.Add(content, "file", name);
        using var response = await client.PostAsync("/api/admin/media", form);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<MediaFileDto>())!;
    }

    private static byte[] PngBytes() => [137, 80, 78, 71, 13, 10, 26, 10, 0];

    public async Task DisposeAsync()
    {
        client.Dispose();
        await factory.DeleteDatabaseAsync();
        await factory.DisposeAsync();
    }
}

internal sealed class MediaApiFactory : WebApplicationFactory<Program>
{
    private readonly string connectionString = CreateConnectionString();
    private readonly string mediaPath = Path.Combine(Path.GetTempPath(), $"portfolio-media-tests-{Guid.NewGuid():N}");
    public Guid ReferencedMediaId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:PortfolioDatabase", connectionString);
        builder.UseSetting("Media:LocalPath", mediaPath);
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
        await db.Database.MigrateAsync();
        var media = MediaFile.Create("referenced.png", "referenced.png", "/media/referenced.png", "image/png", 9, "local");
        var project = Project.Create("Media project", "media-project", "A project using reusable media.");
        project.UpdateContent("Media project", "media-project", "A project using reusable media.", null, null, null, null, null, null, null, null, media.Id, null);
        db.MediaFiles.Add(media);
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        ReferencedMediaId = media.Id;
    }

    public async Task DeleteDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PortfolioDbContext>().Database.EnsureDeletedAsync();
        if (Directory.Exists(mediaPath)) Directory.Delete(mediaPath, true);
    }

    private static string CreateConnectionString()
    {
        var databaseName = $"PortfolioMediaTests_{Guid.NewGuid():N}";
        var configuredConnection = Environment.GetEnvironmentVariable("PORTFOLIO_TEST_SQL_CONNECTION_STRING");
        return string.IsNullOrWhiteSpace(configuredConnection)
            ? $"Server=(localdb)\\PortfolioPlatformLocal;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
            : $"{configuredConnection.TrimEnd(';')};Database={databaseName}";
    }
}
