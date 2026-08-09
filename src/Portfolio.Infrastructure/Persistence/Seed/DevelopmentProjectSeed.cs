using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Seed;

public static class DevelopmentProjectSeed
{
    private sealed record ProjectSeed(
        string Title,
        string Slug,
        string ShortDescription,
        string Description,
        string BusinessProblem,
        string Solution,
        string Architecture,
        string KeyFeatures,
        string Challenges,
        string Impact,
        string LessonsLearned,
        bool Featured,
        string[] Technologies);

    public sealed record Result(int ProjectsAdded, int TechnologiesAdded, int RelationshipsAdded);

    private static readonly (string Name, string Category)[] Technologies =
    [
        (".NET", "Backend"), ("ASP.NET Core", "Backend"), ("EF Core", "Backend"),
        ("LINQ", "Backend"), ("Razor", "Frontend"), ("WebForms", "Frontend"),
        ("SQL Server", "Data"), ("Advanced SQL", "Data"), ("Bootstrap 5", "Frontend"),
        ("Metronic 8.29 RTL", "Frontend"), ("JavaScript", "Frontend"), ("jQuery", "Frontend"),
        ("SignalR", "Integration"), ("REST API", "Integration"), ("SOAP", "Integration"),
        ("ApexCharts.js", "Frontend"), ("SweetAlert2", "Frontend"), ("Lottie", "Frontend"),
        ("Git", "DevOps"), ("Azure Boards", "DevOps"), ("CI/CD", "DevOps"),
        ("OutSystems", "Enterprise Platform"), ("Reactive Web", "Enterprise Platform"),
        ("HTML5", "Frontend"), ("CSS3", "Frontend"), ("Bootstrap", "Frontend"),
        ("Angular", "Frontend"), ("TypeScript", "Frontend"), ("Tailwind CSS", "Frontend"),
        ("ASP.NET Core Web API", "Backend"), ("Playwright", "Quality"), ("Scalar", "Developer Tooling"),
        ("GitHub Actions", "DevOps"), ("Microsoft Dev Tunnels", "Developer Tooling")
    ];

    // Non-featured records are created first. The three featured records are then
    // published in reverse display order so the existing newest-first query returns
    // Request & Approval, Government Web Systems, and RSAF solutions.
    private static readonly ProjectSeed[] Projects =
    [
        new(
            "Secure Exam & Assessment System",
            "secure-exam-assessment-system",
            "Secure assessment workflow supporting randomized questions, automated grading, and optimized data access.",
            "A portfolio-safe summary of a secure examination system developed as part of enterprise web delivery.",
            "Assessment workflows required reliable question selection, grading, and efficient access to structured examination data.",
            "Implemented a focused web workflow for randomized questions, automated grading, and optimized data retrieval.",
            "A server-rendered .NET web application backed by SQL Server, with assessment rules separated from presentation concerns.",
            "Randomized questions\nAutomated grading\nOptimized data access\nStructured assessment workflow",
            "Balancing predictable assessment behavior with efficient access to question and result data.",
            "Provided a maintainable assessment workflow without exposing internal operational or security details.",
            "Security descriptions should remain factual and avoid implying undocumented cryptographic or certification guarantees.",
            false,
            [".NET", "ASP.NET Core", "SQL Server", "LINQ"]),
        new(
            "Tajneed Frontend Experience",
            "tajneed-frontend-experience",
            "Responsive frontend implementation for a government recruitment experience using HTML5, CSS3, Bootstrap, and JavaScript.",
            "A public-safe frontend case study based on UI implementation work for the Tajneed experience.",
            "The interface needed consistent responsive behavior and clear presentation across government recruitment journeys.",
            "Delivered responsive pages and interface behavior with standards-based frontend technologies.",
            "A browser-focused presentation layer implemented with HTML5, CSS3, Bootstrap, and JavaScript.",
            "Responsive page layouts\nReusable Bootstrap patterns\nClient-side interactions\nCross-screen presentation",
            "Supporting a substantial public-sector interface while keeping presentation consistent and maintainable.",
            "Contributed the frontend implementation without claiming unsupported backend ownership.",
            "Public case studies should describe the frontend contribution accurately and avoid internal Ministry of Defense details.",
            false,
            ["HTML5", "CSS3", "Bootstrap", "JavaScript"]),
        new(
            "Sultan Alomran Portfolio Platform",
            "sultan-alomran-portfolio-platform",
            "Full-stack engineering portfolio built with Angular, ASP.NET Core, EF Core, SQL Server, and automated Playwright quality checks.",
            "The repository-verifiable platform presenting projects, technical content, and engineering-quality telemetry through separate public, admin, and API applications.",
            "A senior engineering portfolio needed to demonstrate real architecture and delivery practices rather than rely on a generic static template.",
            "Built separate Angular public and Metronic-based admin applications over an ASP.NET Core API and SQL Server persistence layer.",
            "Clean Architecture solution boundaries, feature-oriented implementation, typed Angular clients, EF Core persistence, and automated browser testing.",
            "Public Projects experience\nMetronic Admin shell\nScalar API reference\nPlaywright E2E foundation\nPrivate Dev Tunnel workflow\nTest analytics dashboard",
            "Maintaining two intentional Angular versions while preserving shared API behavior and repeatable development workflows.",
            "The implemented platform provides a maintainable public experience, content-management foundation, API documentation, and automated quality checks.",
            "The case study must distinguish implemented repository capabilities from future roadmap items.",
            false,
            ["Angular", "TypeScript", "Tailwind CSS", "ASP.NET Core Web API", "EF Core", "SQL Server", "Playwright", "Scalar", "GitHub Actions", "Microsoft Dev Tunnels"]),
        new(
            "RSAF OutSystems Solutions",
            "rsaf-outsystems-solutions",
            "Portfolio summary of three enterprise OutSystems Reactive Web solutions with workflows, validations, integrations, and reusable application logic.",
            "A portfolio-safe summary of leading delivery across three RSAF OutSystems Reactive Web solutions, focused on engineering practices rather than internal operational details.",
            "Enterprise workflows required robust validations, role-aware behavior, system integrations, and maintainable modular implementation.",
            "Delivered Reactive Web solutions using reusable logic, REST and SOAP integrations, Advanced SQL, and enterprise architecture practices.",
            "OutSystems Reactive Web applications structured with modular enterprise patterns and 4-Layer Canvas principles.",
            "Reactive Web experiences\nWorkflow logic and validations\nRole-based access\nREST and SOAP integrations\nReusable application logic\nAdvanced SQL",
            "Supporting complex enterprise behavior while protecting sensitive operational context.",
            "Delivered three enterprise solutions with reusable logic and integration-focused engineering.",
            "Public presentation should remain generalized and omit application names, operational roles, and infrastructure details.",
            true,
            ["OutSystems", "Reactive Web", "REST API", "SOAP", "Advanced SQL"]),
        new(
            "Government Web Systems Portfolio",
            "government-web-systems-portfolio",
            "Representative portfolio of government web-system delivery across multiple applications using .NET, MVC, Razor, WebForms, and SQL Server.",
            "An aggregate case study representing delivery across seven government projects. It is intentionally presented as a portfolio summary, not as one literal application.",
            "Multiple government web initiatives required dependable server-rendered interfaces, business workflows, and data-backed application behavior.",
            "Contributed full-stack delivery across applications using .NET web technologies, Razor, MVC, WebForms, and SQL Server.",
            "A representative collection of server-rendered .NET web systems with relational persistence and maintainable application boundaries.",
            "Seven-project delivery portfolio\nRazor and MVC applications\nWebForms systems\nSQL-backed workflows\nResponsive government interfaces",
            "Describing a broad body of work meaningfully without combining distinct systems or exposing confidential application names.",
            "Demonstrates sustained delivery across seven government projects using several generations of the .NET web stack.",
            "Aggregate portfolio records must remain explicit that they summarize multiple applications.",
            true,
            [".NET", "ASP.NET Core", "Razor", "WebForms", "SQL Server", "JavaScript"]),
        new(
            "Request & Approval Management System",
            "request-approval-management-system",
            "Enterprise request and approval platform supporting multiple request types, multi-step workflows, versioned attachments, real-time updates, and interactive dashboards.",
            "A major full-stack case study led through enterprise delivery, covering configurable request experiences, approval workflows, attachments, notifications, dashboards, integrations, and reusable modules.",
            "Government-style request processing required many request types, configurable multi-step approvals, versioned evidence, role-aware interfaces, auditability, and timely operational visibility.",
            "Built reusable request, approval, file-upload, attachment-versioning, notification, and audit modules; integrated external identity systems and REST APIs; and added SignalR-based real-time approval behavior.",
            "ASP.NET Core MVC with EF Core, LINQ, and SQL Server; Razor and Metronic-based views; Repository-Service pattern, ViewModels, and clean separation of responsibilities.",
            "Multiple request types\nMulti-step approval workflows\nVersioned attachments\nReal-time notifications\nSignalR approval updates\nInteractive ApexCharts.js dashboards\nRole-based UI visibility\nAudit tracking\nReusable file-upload and approval modules\nREST API and external identity integration",
            "Combining configurable workflows, role-aware UX, reusable modules, and integrations while keeping the system understandable and maintainable.",
            "Improved usability and reduced avoidable interaction errors through redesigned CRUD views, role-aware visibility, clear feedback, and real-time updates.",
            "Reusable modules and clean separation support continued enterprise evolution without claiming the formal Clean Architecture pattern.",
            true,
            ["ASP.NET Core", "EF Core", "LINQ", "SQL Server", "Razor", "Bootstrap 5", "Metronic 8.29 RTL", "JavaScript", "jQuery", "SignalR", "REST API", "ApexCharts.js", "SweetAlert2", "Lottie", "Git", "Azure Boards", "CI/CD"])
    ];

    public static async Task<Result> SeedAsync(IServiceProvider services, string connectionString, CancellationToken cancellationToken = default)
    {
        EnsureLocalDatabase(connectionString);
        using var scope = services.CreateScope();
        return await SeedAsync(scope.ServiceProvider.GetRequiredService<PortfolioDbContext>(), cancellationToken);
    }

    public static async Task<Result> SeedAsync(PortfolioDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var existingTechnologies = await dbContext.Technologies.ToListAsync(cancellationToken);
        var technologyByName = existingTechnologies.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        var technologiesAdded = 0;

        foreach (var seed in Technologies)
        {
            if (technologyByName.ContainsKey(seed.Name)) continue;
            var technology = Technology.Create(seed.Name, seed.Category);
            dbContext.Technologies.Add(technology);
            technologyByName.Add(seed.Name, technology);
            technologiesAdded++;
        }

        if (technologiesAdded > 0) await dbContext.SaveChangesAsync(cancellationToken);

        var existingSlugValues = await dbContext.Projects.IgnoreQueryFilters()
            .Select(x => x.Slug).ToListAsync(cancellationToken);
        var existingSlugs = new HashSet<string>(existingSlugValues, StringComparer.OrdinalIgnoreCase);
        var projectsAdded = 0;
        var relationshipsAdded = 0;

        foreach (var seed in Projects)
        {
            if (existingSlugs.Contains(seed.Slug)) continue;
            var project = Project.Create(seed.Title, seed.Slug, seed.ShortDescription);
            project.UpdateContent(seed.Title, seed.Slug, seed.ShortDescription, seed.Description,
                seed.BusinessProblem, seed.Solution, seed.Architecture, seed.KeyFeatures, seed.Challenges,
                seed.Impact, seed.LessonsLearned, null, null);
            project.SetFeatured(seed.Featured);
            project.Publish();
            foreach (var technologyName in seed.Technologies)
            {
                project.ProjectTechnologies.Add(ProjectTechnology.Create(technologyByName[technologyName].Id));
                relationshipsAdded++;
            }
            dbContext.Projects.Add(project);
            existingSlugs.Add(seed.Slug);
            projectsAdded++;
        }

        if (projectsAdded > 0) await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(projectsAdded, technologiesAdded, relationshipsAdded);
    }

    private static void EnsureLocalDatabase(string connectionString)
    {
        var dataSource = new SqlConnectionStringBuilder(connectionString).DataSource;
        if (!dataSource.StartsWith("(localdb)\\", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Development project seed is restricted to a SQL Server LocalDB data source.");
    }
}
