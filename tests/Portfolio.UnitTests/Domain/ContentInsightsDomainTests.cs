using Portfolio.Domain.Entities;

namespace Portfolio.UnitTests.Domain;

public sealed class ContentInsightsDomainTests
{
    [Fact]
    public void InfographicView_Create_sets_required_properties()
    {
        var infographicId = Guid.NewGuid();
        var visitorHash = new string('F', 64);

        var view = InfographicView.Create(infographicId, visitorHash);

        Assert.Equal(infographicId, view.InfographicId);
        Assert.Equal(visitorHash, view.VisitorKeyHash);
        Assert.True((DateTime.UtcNow - view.CreatedAt).TotalSeconds < 5);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void InfographicView_Create_rejects_empty_visitor_hash(string? invalidHash)
    {
        var infographicId = Guid.NewGuid();
        Assert.Throws<ArgumentException>(() => InfographicView.Create(infographicId, invalidHash!));
    }
}
