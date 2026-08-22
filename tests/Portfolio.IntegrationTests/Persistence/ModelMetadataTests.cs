using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.IntegrationTests.Persistence;

public sealed class ModelMetadataTests
{
    private static IModel Model => new PortfolioDbContext(new DbContextOptionsBuilder<PortfolioDbContext>()
        .UseSqlServer("Server=(local);Database=metadata;Trusted_Connection=True;TrustServerCertificate=True")
        .Options).Model;

    [Fact]
    public void Complete_model_has_configuration_for_every_entity() => Assert.Equal(49, Model.GetEntityTypes().Count());

    [Fact]
    public void Arabic_capable_fields_are_unicode_and_bounded()
    {
        var title = Model.FindEntityType(typeof(Infographic))!.FindProperty(nameof(Infographic.Title))!;
        Assert.True(title.IsUnicode()); Assert.Equal(250, title.GetMaxLength());
    }

    [Fact]
    public void Only_approved_content_has_soft_delete_filters()
    {
        Type[] filtered = [typeof(Category), typeof(Infographic), typeof(Project), typeof(Series), typeof(ReadingPath)];
        Assert.All(filtered, type => Assert.NotEmpty(Model.FindEntityType(type)!.GetDeclaredQueryFilters()));
        Assert.Empty(Model.FindEntityType(typeof(Session))!.GetDeclaredQueryFilters());
    }

    [Fact]
    public void Token_hash_and_active_slug_indexes_are_unique()
    {
        Assert.Contains(Model.FindEntityType(typeof(RefreshToken))!.GetIndexes(), x => x.IsUnique && x.Properties.Single().Name == nameof(RefreshToken.TokenHash));
        Assert.Contains(Model.FindEntityType(typeof(Project))!.GetIndexes(), x => x.IsUnique && x.GetFilter() == "[IsDeleted] = 0");
    }

    [Fact]
    public void External_login_has_unique_provider_identity_and_user_provider_pair()
    {
        var externalLogin = Model.FindEntityType(typeof(UserExternalLogin))!;
        Assert.Contains(externalLogin.GetIndexes(), index => index.IsUnique && index.Properties.Select(x => x.Name)
            .SequenceEqual([nameof(UserExternalLogin.Provider), nameof(UserExternalLogin.ProviderSubject)]));
        Assert.Contains(externalLogin.GetIndexes(), index => index.IsUnique && index.Properties.Select(x => x.Name)
            .SequenceEqual([nameof(UserExternalLogin.UserId), nameof(UserExternalLogin.Provider)]));
    }

    [Fact]
    public void Engagement_has_authenticated_and_anonymous_identity_constraints()
    {
        var bookmark = Model.FindEntityType(typeof(UserBookmark))!;
        Assert.Contains(bookmark.GetIndexes(), index => index.IsUnique && index.Properties.Select(x => x.Name)
            .SequenceEqual(["UserId", "EntityType", "EntityId"]));

        foreach (var type in new[] { typeof(UserHelpfulVote), typeof(UserRating) })
        {
            var entity = Model.FindEntityType(type)!;
            Assert.Contains(entity.GetIndexes(), index => index.IsUnique &&
                index.GetFilter() == "[UserId] IS NOT NULL" &&
                index.Properties.Select(x => x.Name).SequenceEqual(["UserId", "EntityType", "EntityId"]));
            Assert.Contains(entity.GetIndexes(), index => index.IsUnique &&
                index.GetFilter() == "[VisitorKeyHash] IS NOT NULL" &&
                index.Properties.Select(x => x.Name).SequenceEqual(["VisitorKeyHash", "EntityType", "EntityId"]));
            Assert.Equal(64, entity.FindProperty("VisitorKeyHash")!.GetMaxLength());
        }

        using var context = new PortfolioDbContext(new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseSqlServer("Server=(local);Database=metadata;Trusted_Connection=True;TrustServerCertificate=True")
            .Options);
        var designTimeModel = context.GetService<IDesignTimeModel>().Model;
        var ratingChecks = designTimeModel.FindEntityType(typeof(UserRating))!.GetCheckConstraints();
        Assert.Contains(ratingChecks, constraint => constraint.Name == "CK_UserRatings_Rating" &&
            constraint.Sql == "[Rating] BETWEEN 1 AND 5");
        Assert.Contains(ratingChecks, constraint => constraint.Name == "CK_UserRatings_Actor");
        var helpfulChecks = designTimeModel.FindEntityType(typeof(UserHelpfulVote))!.GetCheckConstraints();
        Assert.Contains(helpfulChecks, constraint => constraint.Name == "CK_UserHelpfulVotes_Actor");
        Assert.Contains(helpfulChecks, constraint => constraint.Name == "CK_UserHelpfulVotes_NegativeReason");
    }
    [Fact]
    public void Project_model_supports_featured_case_studies()
    {
        var project = Model.FindEntityType(typeof(Project))!;
        Assert.NotNull(project.FindProperty(nameof(Project.IsFeatured)));
        Assert.NotNull(project.FindProperty(nameof(Project.BusinessProblem)));
        Assert.Contains(project.GetIndexes(), index => index.Properties.Select(x => x.Name)
            .SequenceEqual([nameof(Project.IsFeatured), nameof(Project.Status), nameof(Project.PublishedAt)]));
    }

    [Fact]
    public void Test_telemetry_has_unique_provider_identity_and_external_artifact_metadata()
    {
        var run = Model.FindEntityType(typeof(TestRun))!;
        Assert.Contains(run.GetIndexes(), index => index.IsUnique && index.Properties.Select(x => x.Name)
            .SequenceEqual([nameof(TestRun.Provider), nameof(TestRun.ProviderRunId)]));
        var artifact = Model.FindEntityType(typeof(TestArtifact))!;
        Assert.Null(artifact.FindProperty("BinaryData"));
        Assert.NotNull(artifact.FindProperty(nameof(TestArtifact.ExternalUrl)));
        Assert.NotNull(artifact.FindProperty(nameof(TestArtifact.StoragePath)));
    }

    [Fact]
    public void Media_reference_and_category_delete_behaviors_are_restrict()
    {
        var image = Model.FindEntityType(typeof(ProjectImage))!;
        Assert.Contains(image.GetForeignKeys(), x => x.PrincipalEntityType.ClrType == typeof(MediaFile) && x.DeleteBehavior == DeleteBehavior.Restrict);
        var infographic = Model.FindEntityType(typeof(Infographic))!;
        Assert.Contains(infographic.GetForeignKeys(), x => x.PrincipalEntityType.ClrType == typeof(Category) && x.DeleteBehavior == DeleteBehavior.Restrict);
    }

    [Fact]
    public void Every_entity_has_a_dedicated_configuration()
    {
        var configuredTypes = typeof(PortfolioDbContext).Assembly.GetTypes()
            .SelectMany(type => type.GetInterfaces())
            .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
            .Select(type => type.GenericTypeArguments[0])
            .ToHashSet();

        Assert.All(Model.GetEntityTypes(), entity => Assert.Contains(entity.ClrType, configuredTypes));
    }

    [Fact]
    public void Selective_soft_delete_is_exact_and_uses_sql_server_filters()
    {
        Type[] expected = [typeof(Category), typeof(Infographic), typeof(Project), typeof(Series), typeof(ReadingPath)];
        var filtered = Model.GetEntityTypes().Where(x => x.GetDeclaredQueryFilters().Any()).Select(x => x.ClrType).ToHashSet();
        Assert.True(filtered.SetEquals(expected));
        Assert.All(expected, type => Assert.Contains(Model.FindEntityType(type)!.GetIndexes(),
            index => index.IsUnique && index.GetFilter() == "[IsDeleted] = 0"));
    }

    [Fact]
    public void Sequential_guid_defaults_exist_for_all_entity_keys()
    {
        Assert.All(Model.GetEntityTypes(), entity =>
            Assert.Equal("NEWSEQUENTIALID()", entity.FindProperty("Id")!.GetDefaultValueSql()));
    }

    [Fact]
    public void Approved_shape_excludes_superseded_members()
    {
        Assert.Null(Model.FindEntityType("Portfolio.Domain.Entities.Article"));
        Assert.Null(Model.FindEntityType(typeof(Infographic))!.FindProperty("SeriesId"));
        Assert.Null(Model.FindEntityType(typeof(ProjectImage))!.FindProperty("ImageUrl"));
        Assert.NotNull(Model.FindEntityType(typeof(ProjectImage))!.FindProperty(nameof(ProjectImage.MediaFileId)));
        Assert.DoesNotContain(Model.GetEntityTypes().SelectMany(x => x.GetProperties()), x => x.Name == "PublicCount");
        Assert.NotNull(Model.FindEntityType(typeof(EntityStatistic)));
    }

    [Fact]
    public void Decimal_precision_and_relationship_nullability_match_the_review()
    {
        var ratingAverage = Model.FindEntityType(typeof(EntityStatistic))!.FindProperty(nameof(EntityStatistic.RatingAverage))!;
        Assert.Equal(5, ratingAverage.GetPrecision());
        Assert.Equal(2, ratingAverage.GetScale());

        var projectImage = Model.FindEntityType(typeof(ProjectImage))!;
        Assert.All(projectImage.GetForeignKeys(), foreignKey => Assert.True(foreignKey.IsRequired));
        var pageView = Model.FindEntityType(typeof(PageView))!;
        Assert.All(pageView.GetForeignKeys(), foreignKey => Assert.False(foreignKey.IsRequired));
    }
}
