using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Infographics;
using Portfolio.Application.Projects;

namespace Portfolio.Application.Assistant;

public sealed class PortfolioAssistantService(IProjectsService projects, IInfographicsService infographics,
    IAiAssistantClient client, IOptions<AiAssistantOptions> options, ILogger<PortfolioAssistantService> logger) : IPortfolioAssistantService
{
    public async Task<AssistantMessageResponse> SendAsync(AssistantMessageRequest request, CancellationToken token)
    {
        var settings = options.Value;
        if (!settings.Enabled) throw new AssistantUnavailableException();
        var message = request.Message?.Trim() ?? string.Empty;
        if (message.Length is 0 || message.Length > settings.MaxUserMessageLength) throw new ArgumentException($"Message must contain 1 to {settings.MaxUserMessageLength} characters.");
        if ((request.ConversationContext?.Count ?? 0) > settings.MaxHistoryMessages) throw new ArgumentException($"Conversation context is limited to {settings.MaxHistoryMessages} messages.");
        var evidence = new List<AssistantSource>();
        var lowered = message.ToLowerInvariant();
        if (lowered.Contains("project") || lowered.Contains("angular") || lowered.Contains(".net") || lowered.Contains("api") || lowered.Contains("sql server"))
        {
            var technology = new[] { "Angular", ".NET", "SQL Server", "OutSystems" }.FirstOrDefault(value => message.Contains(value, StringComparison.OrdinalIgnoreCase));
            var result = await projects.GetPublicProjectsAsync(new ProjectQuery(Technology: technology, Page: 1, PageSize: 5), token);
            evidence.AddRange(result.Items.Select(item => new AssistantSource("Project", item.Title, $"/projects/{item.Slug}", item.ShortDescription)));
        }
        if (lowered.Contains("guide") || lowered.Contains("handbook") || lowered.Contains("infographic") || lowered.Contains("ef core") || lowered.Contains("performance"))
        {
            var search = lowered.Contains("ef core") ? "EF Core" : lowered.Contains("performance") ? "performance" : null;
            var result = await infographics.GetPublicAsync(new InfographicQuery(Search: search, Page: 1, PageSize: 5), token);
            evidence.AddRange(result.Items.Select(item => new AssistantSource("Infographic", item.Title, $"/visual-handbook/{item.Slug}", item.ShortDescription)));
        }
        logger.LogInformation("Assistant grounding completed with {EvidenceCount} public sources", evidence.Count);
        return await client.CompleteAsync(new AssistantGrounding(message, request.ConversationContext ?? [], evidence, PublicProfile.Context), token);
    }
}

internal static class PublicProfile
{
    // Approved public facts mirror Portfolio.Web home.data.ts and exclude contact/private details.
    internal const string Context = "Sultan Alomran is a full-stack web developer with 8+ years of experience. His public stack includes C#, ASP.NET Core, Angular, TypeScript, SQL Server, REST APIs and OutSystems. Public certifications: OutSystems Architecture Specialist (2026), OutSystems Associate Reactive Web Developer (2024, 92%), Scrum attendance (Tuwaiq Academy, 2026), and Development using JavaScript (Misk, 2018). Experience: SAMI Advanced Electronics full-stack developer since 2019; frontend developer 2018–2019; web developer and business analyst trainee in 2017.";
}
