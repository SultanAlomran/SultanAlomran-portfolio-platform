using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Portfolio.IntegrationTests;

public sealed class ApiFoundationTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Application_starts_and_health_endpoint_is_healthy()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/health", CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Verification_endpoint_returns_correlation_identifier()
    {
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/api", CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [Fact]
    public async Task Development_exposes_valid_OpenApi_and_Scalar_reference()
    {
        using var client = factory.CreateClient();
        using var openApiResponse = await client.GetAsync("/openapi/v1.json", CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, openApiResponse.StatusCode);

        await using var openApiStream = await openApiResponse.Content.ReadAsStreamAsync(CancellationToken.None);
        using var openApi = await JsonDocument.ParseAsync(openApiStream, cancellationToken: CancellationToken.None);
        Assert.True(openApi.RootElement.TryGetProperty("openapi", out _));
        Assert.True(openApi.RootElement.GetProperty("paths").TryGetProperty("/api", out _));
        Assert.True(openApi.RootElement.GetProperty("paths").TryGetProperty("/api/assistant/messages", out var assistantPath));
        Assert.True(assistantPath.TryGetProperty("post", out _));

        using var scalarResponse = await client.GetAsync("/scalar/", CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, scalarResponse.StatusCode);
        Assert.Equal("text/html", scalarResponse.Content.Headers.ContentType?.MediaType);
        var scalarHtml = await scalarResponse.Content.ReadAsStringAsync(CancellationToken.None);
        Assert.Contains("Portfolio Platform API", scalarHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Production_does_not_expose_OpenApi_or_Scalar_reference()
    {
        await using var productionFactory = factory.WithWebHostBuilder(builder => builder.UseEnvironment("Production"));
        using var client = productionFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/openapi/v1.json", CancellationToken.None)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/scalar/", CancellationToken.None)).StatusCode);
    }
}
