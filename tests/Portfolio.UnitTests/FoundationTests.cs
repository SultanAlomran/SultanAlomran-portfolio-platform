namespace Portfolio.UnitTests;

public sealed class FoundationTests
{
    [Fact]
    public void Shared_assembly_is_available_without_business_types()
    {
        Assert.Equal("Portfolio.Shared", typeof(Portfolio.Shared.AssemblyMarker).Assembly.GetName().Name);
    }
}
