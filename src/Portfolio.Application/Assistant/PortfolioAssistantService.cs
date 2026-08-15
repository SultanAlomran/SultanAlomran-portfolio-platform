using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Portfolio.Application.Assistant;

public sealed class PortfolioAssistantService(IAssistantTools tools, IAiAssistantClient client,
    IOptions<AiAssistantOptions> options, ILogger<PortfolioAssistantService> logger) : IPortfolioAssistantService
{
    public async Task<AssistantMessageResponse> SendAsync(AssistantMessageRequest request, CancellationToken token)
    {
        var settings = options.Value;
        if (!settings.Enabled) throw new AssistantUnavailableException();
        var message = request.Message?.Trim() ?? string.Empty;
        if (message.Length is 0 || message.Length > settings.MaxUserMessageLength)
            throw new ArgumentException($"Message must contain 1 to {settings.MaxUserMessageLength} characters.");
        if ((request.ConversationContext?.Count ?? 0) > settings.MaxHistoryMessages)
            throw new ArgumentException($"Conversation context is limited to {settings.MaxHistoryMessages} messages.");
        if (request.ConversationContext?.Any(item => item.Role is not ("user" or "assistant") || item.Content.Length > settings.MaxUserMessageLength) == true)
            throw new ArgumentException("Conversation context contains an invalid role or oversized message.");

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(settings.RequestTimeoutSeconds, 1, 120)));
        try
        {
            var evidence = await GetEvidenceAsync(message, settings.MaxToolRounds, timeout.Token);
            var grounding = new AssistantGrounding(message, request.ConversationContext ?? [], evidence, PublicProfile.Context);
            var response = await client.CompleteAsync(grounding, timeout.Token);
            if (string.IsNullOrWhiteSpace(response.Message)) throw new AssistantProviderException("The provider returned an invalid response.");
            var outputLimit = Math.Clamp(settings.MaxOutputCharacters, 100, 20_000);
            var output = response.Message.Length <= outputLimit ? response.Message : response.Message[..outputLimit];
            logger.LogInformation("Assistant request completed with {EvidenceCount} sources", evidence.Count);
            return response with { Message = output, Sources = SanitizeSources(response.Sources), Actions = SanitizeActions(response.Actions) };
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            throw new AssistantProviderException("The assistant provider timed out.");
        }
    }

    private async Task<IReadOnlyList<AssistantSource>> GetEvidenceAsync(string message, int maxRounds, CancellationToken token)
    {
        var evidence = new List<AssistantSource>();
        var lower = message.ToLowerInvariant();
        string[] prohibited = ["select ", "insert ", "update ", "delete ", "drop ", "database password", "connection string", "system prompt", "admin", "private data", "unpublished", "secret"];
        if (prohibited.Any(lower.Contains)) return evidence;
        var rounds = 0;
        var slug = ExtractInternalSlug(message);
        if (slug is not null && rounds++ < maxRounds)
        {
            var detail = lower.Contains("handbook") || lower.Contains("guide") || lower.Contains("infographic")
                ? await tools.GetInfographicDetailsAsync(slug, token) : await tools.GetProjectDetailsAsync(slug, token);
            if (detail is not null) evidence.Add(detail);
        }
        if ((lower.Contains("project") || lower.Contains("angular") || lower.Contains(".net") || lower.Contains("api") || lower.Contains("sql server")) && rounds++ < maxRounds)
        {
            var technology = new[] { "Angular", ".NET", "SQL Server", "OutSystems" }.FirstOrDefault(value => message.Contains(value, StringComparison.OrdinalIgnoreCase));
            evidence.AddRange(await tools.SearchProjectsAsync(technology, token));
        }
        if ((lower.Contains("guide") || lower.Contains("handbook") || lower.Contains("infographic") || lower.Contains("ef core") || lower.Contains("performance")) && rounds++ < maxRounds)
        {
            var search = lower.Contains("ef core") ? "EF Core" : lower.Contains("performance") ? "performance" : null;
            evidence.AddRange(await tools.SearchInfographicsAsync(search, token));
        }
        return evidence.DistinctBy(item => item.Route).Take(10).ToArray();
    }

    private static string? ExtractInternalSlug(string message)
    {
        var marker = message.Contains("/visual-handbook/", StringComparison.OrdinalIgnoreCase) ? "/visual-handbook/" :
            message.Contains("/projects/", StringComparison.OrdinalIgnoreCase) ? "/projects/" : null;
        if (marker is null) return null;
        return message[(message.IndexOf(marker, StringComparison.OrdinalIgnoreCase) + marker.Length)..]
            .Split([' ', '?', '#'], StringSplitOptions.RemoveEmptyEntries)[0].Trim('/').ToLowerInvariant();
    }

    private static IReadOnlyList<AssistantSource> SanitizeSources(IEnumerable<AssistantSource> sources) =>
        sources.Where(item => IsSafeRoute(item.Route)).Take(10).ToArray();
    private static IReadOnlyList<AssistantAction> SanitizeActions(IEnumerable<AssistantAction> actions) =>
        actions.Where(item => item.Type == "Navigate" && IsSafeRoute(item.Route)).Take(10).ToArray();
    private static bool IsSafeRoute(string route) => route.StartsWith("/projects", StringComparison.Ordinal) || route.StartsWith("/visual-handbook", StringComparison.Ordinal) || route is "/experience";
}

internal static class PublicProfile
{
    internal const string Context = "Sultan Alomran is a full-stack web developer with 8+ years of experience. His public stack includes C#, ASP.NET Core, Angular, TypeScript, SQL Server, REST APIs and OutSystems. Public certifications: OutSystems Architecture Specialist (2026), OutSystems Associate Reactive Web Developer (2024, 92%), Scrum attendance (Tuwaiq Academy, 2026), and Development using JavaScript (Misk, 2018). Experience: SAMI Advanced Electronics full-stack developer since 2019; frontend developer 2018–2019; web developer and business analyst trainee in 2017. Education and professional development: SQL Server Developer Track (New Horizon, June 2025), ASP.NET Core with MVC and EF Core (March 2025), OutSystems Reactive Web Developer (July 2023), OutSystems Traditional Web Developer (May 2023), and Front-End Web Development Nanodegree (Udacity, 2019).";
}
