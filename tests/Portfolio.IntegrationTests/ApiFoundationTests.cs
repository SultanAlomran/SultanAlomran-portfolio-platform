using System.Net;
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
}
