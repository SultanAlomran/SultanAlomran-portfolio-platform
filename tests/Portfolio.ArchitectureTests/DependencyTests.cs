using System.Reflection;

namespace Portfolio.ArchitectureTests;

public sealed class DependencyTests
{
    [Fact]
    public void Domain_does_not_reference_outer_layers()
    {
        var references = typeof(Portfolio.Domain.AssemblyMarker).Assembly.GetReferencedAssemblies().Select(x => x.Name).ToArray();
        Assert.DoesNotContain("Portfolio.Application", references);
        Assert.DoesNotContain("Portfolio.Infrastructure", references);
        Assert.DoesNotContain("Portfolio.Api", references);
    }

    [Fact]
    public void Application_does_not_reference_host_or_infrastructure()
    {
        var assembly = Assembly.Load("Portfolio.Application");
        var references = assembly.GetReferencedAssemblies().Select(x => x.Name).ToArray();
        Assert.DoesNotContain("Portfolio.Infrastructure", references);
        Assert.DoesNotContain("Portfolio.Api", references);
    }

    [Fact]
    public void Domain_remains_persistence_framework_independent()
    {
        var references = typeof(Portfolio.Domain.AssemblyMarker).Assembly.GetReferencedAssemblies().Select(x => x.Name).ToArray();
        Assert.DoesNotContain(references, x => x is not null && x.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
    }
}
