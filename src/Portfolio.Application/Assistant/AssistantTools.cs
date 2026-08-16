using System.Text.Json;
using Portfolio.Application.Infographics;
using Portfolio.Application.Projects;

namespace Portfolio.Application.Assistant;

public sealed class AssistantTools(IProjectsService projects, IInfographicsService infographics) : IAssistantTools
{
    private const int MaximumPageSize = 5;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public IReadOnlyList<AssistantToolDefinition> Definitions { get; } = CreateDefinitions();

    public async Task<AssistantToolResult> ExecuteAsync(AssistantToolCall call, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        return call.Name switch
        {
            "search_projects" => await SearchProjectsAsync(call, token),
            "get_project_details" => await GetProjectDetailsAsync(call, token),
            "search_infographics" => await SearchInfographicsAsync(call, token),
            "get_infographic_details" => await GetInfographicDetailsAsync(call, token),
            "search_technologies" => await SearchTechnologiesAsync(call, token),
            "get_portfolio_profile" => Static(call, PublicFacts.Profile, [new("profile", "Portfolio profile", "/about")]),
            "get_experience" => Static(call, PublicFacts.Experience, [new("profile", "Experience", "/experience")]),
            "get_certifications" => Static(call, PublicFacts.Certifications, [new("profile", "Certifications", "/experience")]),
            "get_education_and_professional_development" => Static(call, PublicFacts.Education, [new("profile", "Education and professional development", "/experience")]),
            "get_contact_options" => Static(call, PublicFacts.Contact, [new("profile", "Contact options", "/contact")], [new("Contact", "Contact", "/contact"), new("OpenLinkedIn", "LinkedIn", "https://www.linkedin.com/in/sultan-alomran"), new("OpenGitHub", "GitHub", "https://github.com/SultanAlomran"), new("DownloadCv", "Download CV", "/api/profile/cv")]),
            "compare_projects" => await CompareProjectsAsync(call, token),
            "find_related_content" => await FindRelatedContentAsync(call, token),
            _ => throw new AssistantToolException("Unsupported assistant tool.")
        };
    }

    private async Task<AssistantToolResult> SearchProjectsAsync(AssistantToolCall call, CancellationToken token)
    {
        var a = Read<SearchProjectsArgs>(call.Arguments);
        var page = Math.Max(a.Page ?? 1, 1); var size = Math.Clamp(a.PageSize ?? MaximumPageSize, 1, MaximumPageSize);
        var result = await projects.GetPublicProjectsAsync(new ProjectQuery(a.SearchText, a.Technology, Featured: a.Featured, Page: page, PageSize: size), token);
        var rows = result.Items.Select(x => new { x.Slug, x.Title, x.ShortDescription, x.IsFeatured, Technologies = x.Technologies.Select(t => t.Name).Take(12) }).ToArray();
        return Result(call, rows, result.Items.Select(ProjectSource));
    }

    private async Task<AssistantToolResult> GetProjectDetailsAsync(AssistantToolCall call, CancellationToken token)
    {
        var slug = SafeSlug(Read<SlugArgs>(call.Arguments).Slug);
        var item = await projects.GetPublicProjectBySlugAsync(slug, token);
        if (item is null) return Result(call, new { Found = false }, []);
        var output = new
        {
            item.Slug,
            item.Title,
            Summary = item.ShortDescription,
            item.Description,
            item.BusinessProblem,
            item.Solution,
            item.Architecture,
            item.KeyFeatures,
            item.Challenges,
            item.Impact,
            item.LessonsLearned,
            Technologies = item.Technologies.Select(x => new { x.Name, x.Category }).Take(20),
            Links = item.Links.Select(x => new { x.Title, x.Url, x.LinkType }).Take(8),
            Media = item.Images.Select(x => new { x.Url, x.AltText }).Take(6),
            PublicRoute = $"/projects/{item.Slug}"
        };
        return Result(call, output, [ProjectSource(item)]);
    }

    private async Task<AssistantToolResult> SearchInfographicsAsync(AssistantToolCall call, CancellationToken token)
    {
        var a = Read<SearchInfographicsArgs>(call.Arguments);
        Portfolio.Domain.Enums.DifficultyLevel? difficulty = Enum.TryParse<Portfolio.Domain.Enums.DifficultyLevel>(a.Difficulty, true, out var parsed) ? parsed : null;
        var result = await infographics.GetPublicAsync(new InfographicQuery(a.SearchText, a.Category, a.Tag,
            Difficulty: difficulty, Featured: a.Featured, Page: Math.Max(a.Page ?? 1, 1), PageSize: Math.Clamp(a.PageSize ?? MaximumPageSize, 1, MaximumPageSize)), token);
        var rows = result.Items.Select(x => new
        {
            x.Slug,
            x.Title,
            Summary = x.ShortDescription,
            Difficulty = x.DifficultyLevel.ToString(),
            x.IsFeatured,
            Category = x.Category.Name,
            Tags = x.Tags.Select(t => t.Name).Take(12)
        }).ToArray();
        return Result(call, rows, result.Items.Select(InfographicSource));
    }

    private async Task<AssistantToolResult> GetInfographicDetailsAsync(AssistantToolCall call, CancellationToken token)
    {
        var slug = SafeSlug(Read<SlugArgs>(call.Arguments).Slug);
        var item = await infographics.GetPublicBySlugAsync(slug, token);
        if (item is null) return Result(call, new { Found = false }, []);
        var output = new
        {
            item.Slug,
            item.Title,
            Summary = item.ShortDescription,
            item.Description,
            Difficulty = item.DifficultyLevel.ToString(),
            Category = item.Category.Name,
            Tags = item.Tags.Select(x => x.Name).Take(15),
            Steps = item.Steps.Take(10).Select(x => new { x.StepNumber, x.Title, x.Content }),
            CodeExamples = item.CodeExamples.Take(5).Select(x => new { x.Title, x.Language, Code = Bound(x.Code, 2_000) }),
            Resources = item.Resources.Take(8).Select(x => new { x.Title, x.Url, x.ResourceType }),
            Series = item.Series.Take(5),
            Media = new { item.CoverUrl, item.InfographicUrl, item.PdfUrl },
            PublicRoute = $"/visual-handbook/{item.Slug}"
        };
        return Result(call, output, [InfographicSource(item)]);
    }

    private async Task<AssistantToolResult> SearchTechnologiesAsync(AssistantToolCall call, CancellationToken token)
    {
        var a = Read<TechnologyArgs>(call.Arguments);
        var result = await projects.GetPublicProjectsAsync(new ProjectQuery(Technology: a.Technology, Search: a.Category, PageSize: MaximumPageSize), token);
        return Result(call, result.Items.Select(x => new { x.Slug, x.Title, Technologies = x.Technologies.Select(t => new { t.Name, t.Category }) }), result.Items.Select(ProjectSource));
    }

    private async Task<AssistantToolResult> CompareProjectsAsync(AssistantToolCall call, CancellationToken token)
    {
        var args = Read<CompareArgs>(call.Arguments);
        var items = new List<ProjectDetailsDto>();
        foreach (var slug in (args.Slugs ?? []).Distinct(StringComparer.OrdinalIgnoreCase).Take(4))
        { var item = await projects.GetPublicProjectBySlugAsync(SafeSlug(slug), token); if (item is not null) items.Add(item); }
        var output = items.Select(x => new { x.Slug, x.Title, args.Focus, x.Architecture, x.KeyFeatures, x.Impact, Technologies = x.Technologies.Select(t => t.Name).Take(15) });
        return Result(call, output, items.Select(ProjectSource));
    }

    private async Task<AssistantToolResult> FindRelatedContentAsync(AssistantToolCall call, CancellationToken token)
    {
        var a = Read<RelatedArgs>(call.Arguments);
        var projectResult = await projects.GetPublicProjectsAsync(new ProjectQuery(a.SearchText, a.Technology, PageSize: 3), token);
        var guideResult = await infographics.GetPublicAsync(new InfographicQuery(a.SearchText, a.Category, a.Tag, PageSize: 3), token);
        var sources = projectResult.Items.Select(ProjectSource).Concat(guideResult.Items.Select(InfographicSource)).ToArray();
        return Result(call, new { Projects = projectResult.Items.Select(x => new { x.Slug, x.Title }), Guides = guideResult.Items.Select(x => new { x.Slug, x.Title }) }, sources);
    }

    private static AssistantToolResult Static(AssistantToolCall call, object output, IReadOnlyList<AssistantSource> sources, IReadOnlyList<AssistantAction>? actions = null) => Result(call, output, sources, actions);
    private static AssistantToolResult Result(AssistantToolCall call, object output, IEnumerable<AssistantSource> sources, IReadOnlyList<AssistantAction>? actions = null) =>
        new(call.Id, call.Name, JsonSerializer.SerializeToElement(output, JsonOptions), sources.Take(10).ToArray(), actions);
    private static T Read<T>(JsonElement value) => JsonSerializer.Deserialize<T>(value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null ? "{}" : value.GetRawText(), JsonOptions) ?? throw new AssistantToolException("Malformed tool request.");
    private static string SafeSlug(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 120 && value.All(c => char.IsLetterOrDigit(c) || c == '-') ? value : throw new AssistantToolException("Invalid public identifier.");
    private static string Bound(string value, int max) => value.Length <= max ? value : value[..max];
    private static AssistantSource ProjectSource(ProjectListItemDto x) => new("project", x.Title, $"/projects/{x.Slug}", x.ShortDescription);
    private static AssistantSource ProjectSource(ProjectDetailsDto x) => new("project", x.Title, $"/projects/{x.Slug}", x.ShortDescription);
    private static AssistantSource InfographicSource(InfographicListItemDto x) => new("infographic", x.Title, $"/visual-handbook/{x.Slug}", x.ShortDescription);
    private static AssistantSource InfographicSource(InfographicDetailsDto x) => new("infographic", x.Title, $"/visual-handbook/{x.Slug}", x.ShortDescription);

    private static IReadOnlyList<AssistantToolDefinition> CreateDefinitions()
    {
        static AssistantToolDefinition Tool(string name, string description, string schema) => new(name, description, JsonDocument.Parse(schema).RootElement.Clone());
        return [            Tool("search_projects", "Search published public projects. Use for portfolio claims about projects or technology usage.", """{"type":"object","properties":{"searchText":{"type":["string","null"]},"technology":{"type":["string","null"]},"category":{"type":["string","null"]},"featured":{"type":["boolean","null"]},"page":{"type":"integer","minimum":1},"pageSize":{"type":"integer","minimum":1,"maximum":5}},"required":["searchText","technology","category","featured","page","pageSize"],"additionalProperties":false}"""),
            Tool("get_project_details", "Get bounded public details for one published project selected by slug.", """{"type":"object","properties":{"slug":{"type":"string"}},"required":["slug"],"additionalProperties":false}"""),            Tool("search_infographics", "Search published Visual Handbook guides.", """{"type":"object","properties":{"searchText":{"type":["string","null"]},"category":{"type":["string","null"]},"tag":{"type":["string","null"]},"difficulty":{"type":["string","null"]},"featured":{"type":["boolean","null"]},"page":{"type":"integer","minimum":1},"pageSize":{"type":"integer","minimum":1,"maximum":5}},"required":["searchText","category","tag","difficulty","featured","page","pageSize"],"additionalProperties":false}"""),
            Tool("get_infographic_details", "Get bounded public details for one published Visual Handbook guide.", """{"type":"object","properties":{"slug":{"type":"string"}},"required":["slug"],"additionalProperties":false}"""),            Tool("search_technologies", "Find grounded published project usage of a technology or category.", """{"type":"object","properties":{"technology":{"type":["string","null"]},"category":{"type":["string","null"]}},"required":["technology","category"],"additionalProperties":false}"""),
            Tool("get_portfolio_profile", "Get approved public profile facts.", """{"type":"object","properties":{},"additionalProperties":false}"""),
            Tool("get_experience", "Get approved public career timeline.", """{"type":"object","properties":{},"additionalProperties":false}"""),
            Tool("get_certifications", "Get approved factual public certifications.", """{"type":"object","properties":{},"additionalProperties":false}"""),
            Tool("get_education_and_professional_development", "Get approved education, courses, and professional development facts.", """{"type":"object","properties":{},"additionalProperties":false}"""),
            Tool("get_contact_options", "Get approved public contact and navigation options.", """{"type":"object","properties":{},"additionalProperties":false}"""),
            Tool("compare_projects", "Retrieve bounded structured public data for comparing two to four projects.", """{"type":"object","properties":{"slugs":{"type":"array","items":{"type":"string"},"minItems":2,"maxItems":4},"focus":{"type":["string","null"]}},"required":["slugs","focus"],"additionalProperties":false}"""),
            Tool("find_related_content", "Find related projects and guides by existing structured metadata.", """{"type":"object","properties":{"searchText":{"type":["string","null"]},"technology":{"type":["string","null"]},"category":{"type":["string","null"]},"tag":{"type":["string","null"]}},"required":["searchText","technology","category","tag"],"additionalProperties":false}""")];
    }

    private sealed record SearchProjectsArgs(string? SearchText = null, string? Technology = null, string? Category = null, bool? Featured = null, int? Page = null, int? PageSize = null);
    private sealed record SearchInfographicsArgs(string? SearchText = null, string? Category = null, string? Tag = null, string? Difficulty = null, bool? Featured = null, int? Page = null, int? PageSize = null);
    private sealed record SlugArgs(string? Slug = null);
    private sealed record TechnologyArgs(string? Technology = null, string? Category = null);
    private sealed record CompareArgs(string[]? Slugs = null, string? Focus = null);
    private sealed record RelatedArgs(string? SearchText = null, string? Technology = null, string? Category = null, string? Tag = null);
}

internal static class PublicFacts
{
    internal static readonly object Profile = new { FullName = "Sultan Alomran", Headline = "Senior Full-Stack Software Engineer", Summary = "8+ years building enterprise web solutions with C#, ASP.NET Core, Angular, TypeScript, SQL Server, REST APIs, and OutSystems.", Route = "/about" };
    internal static readonly object Experience = new object[] { new { Role = "Full-stack developer", Organization = "SAMI Advanced Electronics", From = "2019", To = "Present" }, new { Role = "Frontend developer", From = "2018", To = "2019" }, new { Role = "Web developer and business analyst trainee", From = "2017", To = "2017" } };
    internal static readonly object Certifications = new object[] { new { Name = "OutSystems Architecture Specialist", Issuer = "OutSystems", Year = 2026, Score = (int?)null }, new { Name = "OutSystems Associate Reactive Web Developer", Issuer = "OutSystems", Year = 2024, Score = (int?)92 }, new { Name = "Scrum attendance", Issuer = "Tuwaiq Academy", Year = 2026, Score = (int?)null }, new { Name = "Development using JavaScript", Issuer = "Misk", Year = 2018, Score = (int?)null } };
    internal static readonly object Education = new object[] { new { Name = "SQL Server Developer Track", Provider = "New Horizon", Date = "June 2025" }, new { Name = "ASP.NET Core with MVC and EF Core", Date = "March 2025" }, new { Name = "OutSystems Reactive Web Developer", Date = "July 2023" }, new { Name = "OutSystems Traditional Web Developer", Date = "May 2023" }, new { Name = "Front-End Web Development Nanodegree", Provider = "Udacity", Date = "2019" } };
    internal static readonly object Contact = new[] { new { Type = "Contact", Route = "/contact" }, new { Type = "LinkedIn", Route = "https://www.linkedin.com/in/sultan-alomran" }, new { Type = "GitHub", Route = "https://github.com/SultanAlomran" }, new { Type = "DownloadCv", Route = "/api/profile/cv" } };
}
