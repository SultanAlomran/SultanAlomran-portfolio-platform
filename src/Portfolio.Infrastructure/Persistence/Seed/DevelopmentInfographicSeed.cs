using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.Infrastructure.Persistence.Seed;

public static class DevelopmentInfographicSeed
{
    private sealed record Step(string Title, string Content);
    private sealed record Seed(string Title, string Slug, string Summary, string Description,
        string Category, DifficultyLevel Difficulty, bool Featured, bool Published, string[] Tags, Step[] Steps);

    public sealed record Result(int CategoriesAdded, int TagsAdded, int InfographicsAdded, int RelationshipsAdded);

    private static readonly (string Name, string Slug, string Description)[] Categories =
    [
        (".NET", "dotnet", "Practical guidance for modern .NET application engineering."),
        ("Angular", "angular", "Frontend architecture and Angular engineering concepts."),
        ("SQL & Data", "sql-data", "Relational data design, querying, and performance."),
        ("Architecture", "architecture", "System structure, boundaries, and engineering decisions."),
        ("APIs & Integration", "apis-integration", "HTTP, APIs, and dependable system integration."),
        ("OutSystems", "outsystems", "Enterprise Reactive Web and OutSystems architecture guidance.")
    ];

    private static readonly string[] TagNames =
    [
        "EF Core", "Performance", "Angular", "TypeScript", "SQL Server", "Indexes",
        "Clean Architecture", "REST API", "HTTP", "Async", "Background Services",
        "Caching", "OutSystems", "Architecture", ".NET"
    ];

    private static readonly Seed[] Items =
    [
        new("Background Services in .NET", "background-services-dotnet",
            "A practical guide to hosted services, cancellation, scope boundaries, and reliable background work.",
            "Understand how .NET background services fit into an application and how to keep long-running work observable and safe.",
            ".NET", DifficultyLevel.Intermediate, false, false, ["Background Services", "Async", "Architecture"],
            [new("Choose the right workload", "Use hosted services for process-local recurring or queued work, not as a replacement for a durable distributed job system."), new("Respect cancellation", "Propagate the host cancellation token and stop promptly during deployment or shutdown."), new("Create dependency scopes", "Resolve scoped services inside an explicit scope for each unit of work."), new("Observe failures", "Log bounded context and make retry behavior intentional rather than infinite.")]),
        new("Caching Without Stale Surprises", "caching-without-stale-surprises",
            "A decision guide for cache boundaries, keys, expiration, invalidation, and operational visibility.",
            "Caching improves latency only when ownership and invalidation rules remain understandable.",
            "Architecture", DifficultyLevel.Intermediate, false, true, ["Caching", "Performance", "Architecture"],
            [new("Measure first", "Confirm the bottleneck and the acceptable freshness window before introducing a cache."), new("Design stable keys", "Include every input that changes the result and keep tenant or user boundaries explicit."), new("Choose expiration", "Use an expiration policy that matches how quickly the source changes."), new("Plan invalidation", "Prefer a small number of observable invalidation paths over hidden cache mutations.")]),
        new("Async and Await: The Execution Model", "async-await-execution-model",
            "Visual guidance for tasks, suspension points, cancellation, and avoiding thread-blocking waits.",
            "Follow an asynchronous operation from the caller through suspension and completion without treating async as automatic parallelism.",
            ".NET", DifficultyLevel.Intermediate, false, true, ["Async", ".NET", "Performance"],
            [new("Start with I/O", "Async is most valuable when a request waits on database, network, or file I/O."), new("Propagate tasks", "Return and await the task through each application layer instead of blocking with Result or Wait."), new("Carry cancellation", "Pass cancellation tokens through API, application, and persistence boundaries."), new("Keep concurrency bounded", "Parallel work still needs explicit limits and predictable failure handling.")]),
        new("HTTP to REST API: A Request Journey", "http-rest-api-request-journey",
            "Trace an HTTP request through routing, validation, application logic, persistence, and a stable response.",
            "A concise request lifecycle for designing APIs that remain understandable across frontend and backend boundaries.",
            "APIs & Integration", DifficultyLevel.Beginner, false, true, ["REST API", "HTTP", "Architecture"],
            [new("Define the contract", "Choose a resource-oriented route, request shape, response shape, and expected status codes."), new("Validate at the boundary", "Reject malformed or semantically invalid input with a stable problem response."), new("Run the use case", "Keep transport code thin while application logic coordinates the operation."), new("Return intentionally", "Use status codes and response metadata that callers can handle predictably.")]),
        new("OutSystems 4-Layer Canvas", "outsystems-four-layer-canvas",
            "Organize enterprise OutSystems solutions into foundation, core, orchestration, and experience responsibilities.",
            "A portfolio-safe architecture guide to modular Reactive Web delivery and controlled dependencies.",
            "OutSystems", DifficultyLevel.Advanced, false, true, ["OutSystems", "Architecture", "REST API"],
            [new("Foundation", "Keep reusable integrations and shared technical capabilities stable and dependency-light."), new("Core", "Own durable business concepts and reusable domain behavior."), new("Orchestration", "Coordinate processes and cross-domain use cases without leaking UI concerns."), new("Experience", "Build role-appropriate Reactive Web experiences over the lower layers.")]),
        new("SQL Server Indexing Guide", "sql-server-indexing-guide",
            "Understand how access patterns, selectivity, key order, includes, and maintenance shape useful indexes.",
            "A practical visual checklist for improving SQL Server query access without accumulating redundant indexes.",
            "SQL & Data", DifficultyLevel.Intermediate, true, true, ["SQL Server", "Indexes", "Performance"],
            [new("Start from the query", "Inspect predicates, joins, ordering, and the columns returned by the workload."), new("Order keys intentionally", "Place equality and selective predicates according to real access patterns."), new("Cover selectively", "Use included columns when they remove expensive lookups without making the index excessively wide."), new("Verify the plan", "Compare actual execution plans and logical reads before and after the change."), new("Maintain deliberately", "Monitor usage and fragmentation instead of rebuilding every index indiscriminately.")]),
        new("Angular Change Detection", "angular-change-detection",
            "A visual model of component checks, immutable state, signals, and focused rendering in modern Angular.",
            "Understand how data changes reach the template and how component boundaries keep frontend work predictable.",
            "Angular", DifficultyLevel.Intermediate, true, true, ["Angular", "TypeScript", "Performance"],
            [new("Model state explicitly", "Keep state ownership clear and expose the smallest reactive surface a component needs."), new("Prefer stable inputs", "Immutable values and focused component inputs make changes easier to reason about."), new("Use signals intentionally", "Derive computed state rather than synchronizing duplicate mutable values."), new("Measure rendering", "Use browser and Angular tooling before applying optimization patterns.")]),
        new("EF Core Performance Checklist", "ef-core-performance-checklist",
            "Practical query-shaping guidance for projection, tracking, pagination, indexes, and avoiding N+1 access.",
            "A structured checklist for reviewing EF Core read paths before reaching for premature caching or low-level optimizations.",
            ".NET", DifficultyLevel.Intermediate, true, true, ["EF Core", "Performance", "SQL Server"],
            [new("Project the response", "Select only the columns required by the API contract instead of materializing full entity graphs."), new("Disable tracking for reads", "Use AsNoTracking when the result will not be updated in the current unit of work."), new("Bound every list", "Apply server-side filtering, stable sorting, and pagination before materialization."), new("Watch relationship access", "Prefer one translated projection over per-row relationship queries that create N+1 traffic."), new("Validate with SQL", "Inspect generated SQL and query plans against realistic data before declaring an optimization complete.")])
    ];

    public static async Task<Result> SeedAsync(
        IServiceProvider services,
        string connectionString,
        bool allowRemoteDatabase = false,
        CancellationToken token = default)
    {
        EnsureDatabaseAllowed(connectionString, allowRemoteDatabase);
        using var scope = services.CreateScope();
        return await SeedAsync(scope.ServiceProvider.GetRequiredService<PortfolioDbContext>(), token);
    }

    public static async Task<Result> SeedAsync(PortfolioDbContext db, CancellationToken token = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(token);
        var categories = (await db.Categories.IgnoreQueryFilters().ToListAsync(token)).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        var tags = (await db.Tags.ToListAsync(token)).ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        var categoriesAdded = 0; var tagsAdded = 0; var relationships = 0;
        foreach (var (name, slug, description) in Categories)
        {
            if (categories.ContainsKey(name)) continue;
            var item = Category.Create(name, slug, description, categories.Count);
            db.Categories.Add(item); categories.Add(name, item); categoriesAdded++;
        }
        foreach (var name in TagNames)
        {
            if (tags.ContainsKey(name)) continue;
            var item = Tag.Create(name, Slugify(name));
            db.Tags.Add(item); tags.Add(name, item); tagsAdded++;
        }
        if (categoriesAdded + tagsAdded > 0) await db.SaveChangesAsync(token);

        var existingSlugs = new HashSet<string>(await db.Infographics.IgnoreQueryFilters().Select(x => x.Slug).ToListAsync(token), StringComparer.OrdinalIgnoreCase);
        var infographicsAdded = 0;
        foreach (var seed in Items)
        {
            if (existingSlugs.Contains(seed.Slug)) continue;
            var category = categories[seed.Category];
            var item = Infographic.Create(seed.Title, seed.Slug, seed.Summary, category.Id, seed.Difficulty);
            item.UpdateContent(seed.Title, seed.Slug, seed.Summary, seed.Description, category.Id, seed.Difficulty,
                seed.Featured, null, null, null);
            foreach (var tagName in seed.Tags) { item.InfographicTags.Add(InfographicTag.Create(tags[tagName].Id)); relationships++; }
            for (var index = 0; index < seed.Steps.Length; index++)
            {
                var step = seed.Steps[index];
                item.Steps.Add(InfographicStep.Create(index + 1, step.Title, step.Content, null, index));
                relationships++;
            }
            if (seed.Published) item.Publish();
            db.Infographics.Add(item); existingSlugs.Add(seed.Slug); infographicsAdded++;
        }
        if (infographicsAdded > 0) await db.SaveChangesAsync(token);
        await transaction.CommitAsync(token);
        return new(categoriesAdded, tagsAdded, infographicsAdded, relationships);
    }

    private static string Slugify(string value) => value.ToLowerInvariant().Replace("&", "and")
        .Replace(".", "dot").Replace(" ", "-").Replace("/", "-");

    private static void EnsureDatabaseAllowed(string connectionString, bool allowRemoteDatabase)
    {
        if (allowRemoteDatabase) return;
        var dataSource = new SqlConnectionStringBuilder(connectionString).DataSource;
        if (!dataSource.StartsWith("(localdb)\\", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Development infographic seed is restricted to a SQL Server LocalDB data source.");
    }
}
