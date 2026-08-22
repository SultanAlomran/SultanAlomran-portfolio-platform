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
    [Fact]
    public async Task Public_discovery_uses_published_metadata_and_deterministic_series_order()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();

        Infographic Guide(string title, string slug, bool published)
        {
            var guide = Infographic.Create(title, slug, $"{title} summary.", factory.CategoryId, DifficultyLevel.Intermediate);
            guide.UpdateContent(title, slug, $"{title} summary.", $"{title} details.", factory.CategoryId,
                DifficultyLevel.Intermediate, false, null, null, null);
            guide.InfographicTags.Add(InfographicTag.Create(factory.TagId));
            if (published) guide.Publish();
            return guide;
        }

        var previous = Guide("Previous Guide", "previous-guide", true);
        var current = Guide("Current Guide", "current-guide", true);
        var next = Guide("Next Guide", "next-guide", true);
        var draft = Guide("Draft Guide", "draft-guide", false);
        var series = Series.Create("Performance Path", "performance-path", displayOrder: 1);
        db.Infographics.AddRange(previous, current, next, draft);
        db.Series.Add(series);
        await db.SaveChangesAsync();
        db.SeriesItems.AddRange(
            SeriesItem.Create(series.Id, previous.Id, 1),
            SeriesItem.Create(series.Id, current.Id, 2),
            SeriesItem.Create(series.Id, next.Id, 3),
            SeriesItem.Create(series.Id, draft.Id, 4));
        await db.SaveChangesAsync();

        var details = await client.GetFromJsonAsync<InfographicDetailsDto>("/api/infographics/current-guide");
        Assert.NotNull(details);
        Assert.Equal(previous.Id, details.Previous?.Id);
        Assert.Equal(next.Id, details.Next?.Id);
        Assert.DoesNotContain(details.Related, item => item.Id == current.Id || item.Id == draft.Id);
        Assert.Contains(details.Related, item => item.Id == previous.Id);
        Assert.Contains(details.Related, item => item.Id == next.Id);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/infographics/draft-guide")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync("/api/infographics/draft-guide/engagement")).StatusCode);

        var resolved = await client.GetFromJsonAsync<IReadOnlyList<InfographicListItemDto>>(
            $"/api/infographics/by-ids?ids={next.Id}&ids={draft.Id}&ids={current.Id}&ids={Guid.NewGuid()}");
        Assert.Equal([next.Id, current.Id], resolved!.Select(item => item.Id).ToArray());

        var tooMany = string.Join("&", Enumerable.Range(0, 51).Select(_ => $"ids={Guid.NewGuid()}"));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"/api/infographics/by-ids?{tooMany}")).StatusCode);
    }

    [Fact]
    public async Task Anonymous_engagement_upserts_votes_ratings_and_real_aggregates()
    {
        Guid infographicId;
        const string slug = "anonymous-engagement-guide";
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
            var guide = Infographic.Create("Anonymous Engagement Guide", slug,
                "A published guide used for deterministic engagement tests.", factory.CategoryId,
                DifficultyLevel.Intermediate);
            guide.UpdateContent(guide.Title, slug, guide.ShortDescription, "Engagement details.",
                factory.CategoryId, DifficultyLevel.Intermediate, false, null, null, null);
            guide.Publish();
            db.Infographics.Add(guide);
            await db.SaveChangesAsync();
            infographicId = guide.Id;
        }

        var empty = await client.GetFromJsonAsync<InfographicEngagementDto>(
            $"/api/infographics/{slug}/engagement");
        Assert.NotNull(empty);
        Assert.Equal(0, empty.HelpfulCount);
        Assert.Equal(0, empty.RatingCount);
        Assert.Null(empty.HelpfulPercentage);
        Assert.Null(empty.AverageRating);
        Assert.Equal([5, 4, 3, 2, 1], empty.RatingDistribution.Select(x => (int)x.Rating).ToArray());

        using var helpfulResponse = await client.PutAsJsonAsync(
            $"/api/infographics/{infographicId}/helpful-vote", new SetHelpfulVoteRequest(true, null));
        Assert.Equal(HttpStatusCode.OK, helpfulResponse.StatusCode);
        var cookie = Assert.Single(helpfulResponse.Headers.GetValues("Set-Cookie"));
        Assert.Contains(".Portfolio.Engagement=", cookie);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);
        var helpful = await helpfulResponse.Content.ReadFromJsonAsync<InfographicEngagementDto>();
        Assert.Equal(1, helpful!.HelpfulCount);
        Assert.True(helpful.VisitorHelpfulVote);

        var notHelpful = await (await client.PutAsJsonAsync(
            $"/api/infographics/{infographicId}/helpful-vote",
            new SetHelpfulVoteRequest(false, NegativeFeedbackReason.NeedsRealWorldExample)))
            .Content.ReadFromJsonAsync<InfographicEngagementDto>();
        Assert.Equal(0, notHelpful!.HelpfulCount);
        Assert.Equal(1, notHelpful.NotHelpfulCount);
        Assert.False(notHelpful.VisitorHelpfulVote);
        Assert.Equal(NegativeFeedbackReason.NeedsRealWorldExample,
            notHelpful.VisitorNegativeFeedbackReason);
        Assert.Contains(notHelpful.NegativeFeedback,
            x => x.Reason == NegativeFeedbackReason.NeedsRealWorldExample && x.Count == 1);

        using var invalidReason = await client.PutAsJsonAsync(
            $"/api/infographics/{infographicId}/helpful-vote", new { isHelpful = false, reason = 99 });
        Assert.Equal(HttpStatusCode.BadRequest, invalidReason.StatusCode);
        using var helpfulWithReason = await client.PutAsJsonAsync(
            $"/api/infographics/{infographicId}/helpful-vote",
            new SetHelpfulVoteRequest(true, NegativeFeedbackReason.Other));
        Assert.Equal(HttpStatusCode.BadRequest, helpfulWithReason.StatusCode);

        var changedBack = await (await client.PutAsJsonAsync(
            $"/api/infographics/{infographicId}/helpful-vote", new SetHelpfulVoteRequest(true, null)))
            .Content.ReadFromJsonAsync<InfographicEngagementDto>();
        Assert.True(changedBack!.VisitorHelpfulVote);
        Assert.Null(changedBack.VisitorNegativeFeedbackReason);
        Assert.Empty(changedBack.NegativeFeedback);

        foreach (var invalidRating in new byte[] { 0, 6 })
        {
            using var invalid = await client.PutAsJsonAsync(
                $"/api/infographics/{infographicId}/rating",
                new SetInfographicRatingRequest(invalidRating));
            Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        }

        var firstRating = await (await client.PutAsJsonAsync(
            $"/api/infographics/{infographicId}/rating", new SetInfographicRatingRequest(1)))
            .Content.ReadFromJsonAsync<InfographicEngagementDto>();
        Assert.Equal(1, firstRating!.RatingCount);
        Assert.Equal(1m, firstRating.AverageRating);
        var changedRating = await (await client.PutAsJsonAsync(
            $"/api/infographics/{infographicId}/rating", new SetInfographicRatingRequest(5)))
            .Content.ReadFromJsonAsync<InfographicEngagementDto>();
        Assert.Equal(1, changedRating!.RatingCount);
        Assert.Equal((byte)5, changedRating.VisitorRating);

        using var secondVisitor = factory.CreateClient();
        var secondRating = await (await secondVisitor.PutAsJsonAsync(
            $"/api/infographics/{infographicId}/rating", new SetInfographicRatingRequest(3)))
            .Content.ReadFromJsonAsync<InfographicEngagementDto>();
        Assert.Equal(2, secondRating!.RatingCount);
        Assert.Equal(4m, secondRating.AverageRating);
        Assert.Equal(1, secondRating.RatingDistribution.Single(x => x.Rating == 5).Count);
        Assert.Equal(1, secondRating.RatingDistribution.Single(x => x.Rating == 3).Count);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PortfolioDbContext>();
            var votes = await db.UserHelpfulVotes.Where(x => x.EntityId == infographicId).ToListAsync();
            var ratings = await db.UserRatings.Where(x => x.EntityId == infographicId).ToListAsync();
            Assert.Single(votes);
            Assert.Equal(2, ratings.Count);
            Assert.All(votes.Cast<object>().Concat(ratings), entity =>
            {
                var visitorHash = entity.GetType().GetProperty("VisitorKeyHash")!.GetValue(entity) as string;
                Assert.Equal(64, visitorHash!.Length);
            });
            Assert.All(votes, vote => Assert.Null(vote.UserId));
            Assert.All(ratings, rating => Assert.Null(rating.UserId));
        }

        using var missing = await client.PutAsJsonAsync(
            $"/api/infographics/{Guid.NewGuid()}/rating", new SetInfographicRatingRequest(5));
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
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
