using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Portfolio.Application.Assistant;
using Portfolio.Application.Infographics;
using Portfolio.Application.Media;
using Portfolio.Domain.Enums;

namespace Portfolio.UnitTests.Assistant;

public sealed class GuideAiServiceTests
{
    [Fact]
    public async Task GenerateSummaryAsync_ReturnsSummary_ForPublishedGuide()
    {
        var infographics = new FakeInfographicsService();
        var client = new FakeGuideAiClient();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var media = new FakeMediaStorage();
        var options = Options.Create(new AiAssistantOptions { Enabled = true, Provider = "Deterministic", Model = "local-grounded-v1" });

        var service = new GuideAiService(
            infographics,
            media,
            client,
            cache,
            options,
            NullLogger<GuideAiService>.Instance);

        var result = await service.GenerateSummaryAsync("ef-core-performance-checklist", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("ef-core-performance-checklist", result.GuideSlug);
        Assert.Equal("EF Core Performance Checklist", result.Title);
        Assert.NotEmpty(result.Summary);
        Assert.NotEmpty(result.KeyTakeaways);
        Assert.NotEmpty(result.CommonUses);
        Assert.True(result.IsVisualGrounded);
        Assert.Equal(1, client.InvocationCount);
    }

    [Fact]
    public async Task GenerateSummaryAsync_UsesCache_OnSubsequentCalls()
    {
        var infographics = new FakeInfographicsService();
        var client = new FakeGuideAiClient();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var media = new FakeMediaStorage();
        var options = Options.Create(new AiAssistantOptions { Enabled = true, Provider = "Deterministic", Model = "local-grounded-v1" });

        var service = new GuideAiService(
            infographics,
            media,
            client,
            cache,
            options,
            NullLogger<GuideAiService>.Instance);

        var first = await service.GenerateSummaryAsync("ef-core-performance-checklist", CancellationToken.None);
        var second = await service.GenerateSummaryAsync("ef-core-performance-checklist", CancellationToken.None);

        Assert.Equal(first.Summary, second.Summary);
        Assert.Equal(1, client.InvocationCount);
    }

    [Fact]
    public async Task GenerateSummaryAsync_ThrowsKeyNotFoundException_WhenGuideDoesNotExist()
    {
        var infographics = new FakeInfographicsService();
        var client = new FakeGuideAiClient();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var media = new FakeMediaStorage();
        var options = Options.Create(new AiAssistantOptions { Enabled = true, Provider = "Deterministic" });

        var service = new GuideAiService(
            infographics,
            media,
            client,
            cache,
            options,
            NullLogger<GuideAiService>.Instance);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.GenerateSummaryAsync("non-existent-guide", CancellationToken.None));
    }

    [Fact]
    public async Task GenerateSummaryAsync_ThrowsAssistantUnavailableException_WhenDisabled()
    {
        var infographics = new FakeInfographicsService();
        var client = new FakeGuideAiClient();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var media = new FakeMediaStorage();
        var options = Options.Create(new AiAssistantOptions { Enabled = false });

        var service = new GuideAiService(
            infographics,
            media,
            client,
            cache,
            options,
            NullLogger<GuideAiService>.Instance);

        await Assert.ThrowsAsync<AssistantUnavailableException>(() =>
            service.GenerateSummaryAsync("ef-core-performance-checklist", CancellationToken.None));
    }

    private sealed class FakeInfographicsService : IInfographicsService
    {
        public Task<InfographicDetailsDto?> GetPublicBySlugAsync(string slug, CancellationToken token)
        {
            if (slug != "ef-core-performance-checklist")
                return Task.FromResult<InfographicDetailsDto?>(null);

            var guide = new InfographicDetailsDto(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "EF Core Performance Checklist",
                "ef-core-performance-checklist",
                "Key optimization strategies for EF Core in high-traffic applications.",
                "A complete performance checklist covering query splitting, tracking, and indexing.",
                DifficultyLevel.Intermediate,
                true,
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 1, 10, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 1, 5, 0, 0, 0, DateTimeKind.Utc),
                "/media/infographics/ef-core-cover.png",
                "/media/infographics/ef-core.png",
                "/media/infographics/ef-core.pdf",
                new InfographicCategoryDto(Guid.NewGuid(), ".NET", "dotnet", null),
                [new InfographicTagDto(Guid.NewGuid(), "EF Core", "ef-core")],
                [new InfographicStepDto(Guid.NewGuid(), 1, "Use AsNoTracking", "Disable change tracking for read-only queries.", null, null, 1)],
                [new InfographicResourceDto(Guid.NewGuid(), "EF Core Docs", "https://learn.microsoft.com", "Documentation", 1)],
                [new InfographicCodeExampleDto(Guid.NewGuid(), "AsNoTracking example", "csharp", "var list = await context.Blogs.AsNoTracking().ToListAsync();", null, 1)],
                [],
                null,
                null,
                []);

            return Task.FromResult<InfographicDetailsDto?>(guide);
        }

        public Task<InfographicPagedResult<InfographicListItemDto>> GetPublicAsync(InfographicQuery query, CancellationToken token) =>
            Task.FromResult(new InfographicPagedResult<InfographicListItemDto>([], 1, 10, 0));

        public Task<IReadOnlyList<InfographicListItemDto>> GetPublicByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken token) =>
            Task.FromResult<IReadOnlyList<InfographicListItemDto>>([]);

        public Task<IReadOnlyList<InfographicListItemDto>> GetFeaturedAsync(int count, CancellationToken token) =>
            Task.FromResult<IReadOnlyList<InfographicListItemDto>>([]);

        public Task<InfographicPagedResult<AdminInfographicListItemDto>> GetAdminAsync(InfographicQuery query, CancellationToken token) =>
            Task.FromResult(new InfographicPagedResult<AdminInfographicListItemDto>([], 1, 10, 0));

        public Task<AdminInfographicDetailsDto?> GetAdminByIdAsync(Guid id, CancellationToken token) =>
            Task.FromResult<AdminInfographicDetailsDto?>(null);

        public Task<IReadOnlyList<InfographicCategoryDto>> GetCategoriesAsync(CancellationToken token) =>
            Task.FromResult<IReadOnlyList<InfographicCategoryDto>>([]);

        public Task<IReadOnlyList<InfographicTagDto>> GetTagsAsync(CancellationToken token) =>
            Task.FromResult<IReadOnlyList<InfographicTagDto>>([]);

        public Task<IReadOnlyList<InfographicMediaDto>> GetMediaAsync(CancellationToken token) =>
            Task.FromResult<IReadOnlyList<InfographicMediaDto>>([]);

        public Task<AdminInfographicDetailsDto> CreateAsync(UpsertInfographicRequest request, CancellationToken token) =>
            throw new NotImplementedException();

        public Task<AdminInfographicDetailsDto> UpdateAsync(Guid id, UpsertInfographicRequest request, CancellationToken token) =>
            throw new NotImplementedException();

        public Task<AdminInfographicDetailsDto> SaveDraftAsync(Guid id, CancellationToken token) =>
            throw new NotImplementedException();

        public Task<InfographicPublishReadinessResponse> GetPublishReadinessAsync(Guid id, CancellationToken token) =>
            Task.FromResult(new InfographicPublishReadinessResponse(true, []));

        public Task<AdminInfographicDetailsDto> PublishAsync(Guid id, CancellationToken token) =>
            throw new NotImplementedException();

        public Task<AdminInfographicDetailsDto> ArchiveAsync(Guid id, CancellationToken token) =>
            throw new NotImplementedException();

        public Task DeleteAsync(Guid id, CancellationToken token) =>
            Task.CompletedTask;
    }

    private sealed class FakeGuideAiClient : IGuideAiClient
    {
        public int InvocationCount { get; private set; }

        public Task<GuideAiSummaryResponse> GenerateSummaryAsync(GuideAiSummaryGrounding grounding, CancellationToken token)
        {
            InvocationCount++;
            return Task.FromResult(new GuideAiSummaryResponse(
                grounding.GuideSlug,
                grounding.Title,
                $"Summary of {grounding.Title}",
                ["Key Takeaway 1", "Key Takeaway 2"],
                ["Production API caching", "High-throughput read queries"],
                "Remember to verify indexes in production.",
                grounding.VisualContext is not null,
                DateTime.UtcNow));
        }
    }

    private sealed class FakeMediaStorage : IMediaStorage
    {
        public Task StoreAsync(string key, Stream content, CancellationToken token) => Task.CompletedTask;
        public Task DeleteAsync(string key, CancellationToken token) => Task.CompletedTask;
        public string GetUrl(string key) => $"/media/{key}";
        public Task<byte[]?> ReadBytesAsync(string key, CancellationToken token) =>
            Task.FromResult<byte[]?>([0x89, 0x50, 0x4E, 0x47]);
    }
}
