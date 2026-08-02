using Microsoft.EntityFrameworkCore;
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
    public void Complete_model_has_configuration_for_every_entity() => Assert.Equal(45, Model.GetEntityTypes().Count());

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
        Assert.All(filtered, type => Assert.NotNull(Model.FindEntityType(type)!.GetQueryFilter()));
        Assert.Null(Model.FindEntityType(typeof(Session))!.GetQueryFilter());
    }

    [Fact]
    public void Token_hash_and_active_slug_indexes_are_unique()
    {
        Assert.Contains(Model.FindEntityType(typeof(RefreshToken))!.GetIndexes(), x => x.IsUnique && x.Properties.Single().Name == nameof(RefreshToken.TokenHash));
        Assert.Contains(Model.FindEntityType(typeof(Project))!.GetIndexes(), x => x.IsUnique && x.GetFilter() == "[IsDeleted] = 0");
    }

    [Fact]
    public void Media_reference_and_category_delete_behaviors_are_restrict()
    {
        var image = Model.FindEntityType(typeof(ProjectImage))!;
        Assert.Contains(image.GetForeignKeys(), x => x.PrincipalEntityType.ClrType == typeof(MediaFile) && x.DeleteBehavior == DeleteBehavior.Restrict);
        var infographic = Model.FindEntityType(typeof(Infographic))!;
        Assert.Contains(infographic.GetForeignKeys(), x => x.PrincipalEntityType.ClrType == typeof(Category) && x.DeleteBehavior == DeleteBehavior.Restrict);
    }
}
