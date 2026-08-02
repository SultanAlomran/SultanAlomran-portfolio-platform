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
        var assembly = typeof(Portfolio.Application.DependencyInjection).Assembly;
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

    [Fact]
    public void Forbidden_persistence_and_mapping_packages_are_absent()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetReferencedAssemblies().Append(x.GetName()))
            .Select(x => x.Name)
            .Where(x => x is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("MediatR", assemblies);
        Assert.DoesNotContain("AutoMapper", assemblies);
        Assert.DoesNotContain("Dapper", assemblies);
    }

    [Fact]
    public void Domain_has_no_relational_or_ui_annotations()
    {
        var forbiddenNamespaces = new[]
        {
            "System.ComponentModel.DataAnnotations",
            "System.ComponentModel.DataAnnotations.Schema"
        };

        var attributes = typeof(Portfolio.Domain.AssemblyMarker).Assembly.GetTypes()
            .SelectMany(type => type.GetCustomAttributesData()
                .Concat(type.GetProperties().SelectMany(property => property.GetCustomAttributesData())));

        Assert.DoesNotContain(attributes, attribute => forbiddenNamespaces.Contains(
            attribute.AttributeType.Namespace,
            StringComparer.Ordinal));
    }

    [Fact]
    public void Infrastructure_is_the_only_layer_that_owns_entity_framework()
    {
        var infrastructureAssembly = typeof(Portfolio.Infrastructure.AssemblyMarker).Assembly;
        Assert.Contains(infrastructureAssembly.GetReferencedAssemblies(), reference =>
            reference.Name?.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) == true);
    }
}
