using System.Text.Json;
using Portfolio.Application.Assistant;

namespace Portfolio.Infrastructure.Assistant;

public sealed class DeterministicAiAssistantClient : IAiAssistantClient
{
    public string ProviderName => "Deterministic";
    public string ModelName => "local-grounded-v2";

    public Task<AssistantProviderTurn> CompleteAsync(AssistantProviderRequest request, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var lower = request.Message.ToLowerInvariant();
        var arabic = request.Message.Any(c => c is >= '\u0600' and <= '\u06ff');
        if (request.ToolResults.Count > 0)
        {
            var count = request.ToolResults.SelectMany(x => x.Sources).DistinctBy(x => x.Route).Count();
            var message = arabic ? $"\u0648\u062c\u062f\u062a {count} \u0645\u0635\u0627\u062f\u0631 \u0639\u0627\u0645\u0629 \u0630\u0627\u062a \u0635\u0644\u0629." :
                $"I found {count} relevant public portfolio source{(count == 1 ? "" : "s")}. The answer is grounded in the sources below.";
            return Task.FromResult(new AssistantProviderTurn(message, [], arabic ? ["\u0627\u0639\u0631\u0636 \u0627\u0644\u062a\u0641\u0627\u0635\u064a\u0644", "\u0645\u0627 \u0627\u0644\u0645\u062d\u062a\u0648\u0649 \u0630\u0648 \u0627\u0644\u0635\u0644\u0629\u061f"] : ["Show the details", "What related content is available?"], arabic ? "ar" : "en"));
        }
        if (IsUnsafe(lower))
        {
            var refusal = arabic ? "\u0644\u0627 \u064a\u0645\u0643\u0646\u0646\u064a \u0627\u0644\u0648\u0635\u0648\u0644 \u0625\u0644\u0649 \u0627\u0644\u0623\u0633\u0631\u0627\u0631 \u0623\u0648 \u062a\u0646\u0641\u064a\u0630 SQL \u0623\u0648 \u0623\u064a \u062a\u063a\u064a\u064a\u0631\u0627\u062a." : "I cannot access secrets, private/admin or unpublished content, run SQL, reveal system instructions, or modify portfolio data.";
            return Task.FromResult(new AssistantProviderTurn(refusal, [], Language: arabic ? "ar" : "en"));
        }
        var call = SelectTool(request.Message, lower, request.History);
        if (call is not null) return Task.FromResult(new AssistantProviderTurn(null, [call], Language: arabic ? "ar" : "en"));
        var answer = arabic ? "\u064a\u0645\u0643\u0646\u0646\u064a \u0627\u0644\u0645\u0633\u0627\u0639\u062f\u0629 \u0641\u064a \u0645\u0634\u0627\u0631\u064a\u0639 \u0633\u0644\u0637\u0627\u0646 \u0648\u062e\u0628\u0631\u0627\u062a\u0647 \u0648\u0634\u0647\u0627\u062f\u0627\u062a\u0647." : "I can help with Sultan's projects, experience, certifications, Visual Handbook guides, or a general technical explanation.";
        return Task.FromResult(new AssistantProviderTurn(answer, [], Language: arabic ? "ar" : "en"));
    }

    private static AssistantToolCall? SelectTool(string message, string lower, IReadOnlyList<AssistantHistoryMessage> history)
    {
        static AssistantToolCall Call(string name, object args) => new(Guid.NewGuid().ToString("N"), name, JsonSerializer.SerializeToElement(args));
        if (lower.Contains("certif") || message.Any(c => c is >= '\u0600' and <= '\u06ff')) return Call("get_certifications", new { });
        if (lower.Contains("education") || lower.Contains("course") || lower.Contains("professional development") || message.Any(c => c is >= '\u0600' and <= '\u06ff')) return Call("get_education_and_professional_development", new { });
        if (lower.Contains("contact") || lower.Contains("linkedin") || lower.Contains("github") || lower.Contains("cv") || message.Any(c => c is >= '\u0600' and <= '\u06ff')) return Call("get_contact_options", new { });
        if (lower.Contains("experience") || lower.Contains("senior .net") || lower.Contains("enterprise engineering") || message.Any(c => c is >= '\u0600' and <= '\u06ff')) return Call("get_experience", new { });
        if (lower.Contains("who is") || lower.Contains("profile") || lower.Contains("30 seconds")) return Call("get_portfolio_profile", new { });
        var projectSlug = ExtractSlug(message, "/projects/"); if (projectSlug is not null) return Call("get_project_details", new { slug = projectSlug });
        var guideSlug = ExtractSlug(message, "/visual-handbook/"); if (guideSlug is not null) return Call("get_infographic_details", new { slug = guideSlug });
        if (lower.Contains("guide") || lower.Contains("handbook") || lower.Contains("infographic") || lower.Contains("ef core") || message.Any(c => c is >= '\u0600' and <= '\u06ff')) return Call("search_infographics", new { searchText = lower.Contains("ef core") ? "EF Core" : (string?)null, page = 1, pageSize = 5 });
        if (lower.Contains("technology") || lower.Contains("used angular") || lower.Contains("sql server") || message.Any(c => c is >= '\u0600' and <= '\u06ff')) return Call("search_technologies", new { technology = Technology(message), category = (string?)null });
        if (lower.Contains("project") || lower.Contains(".net") || lower.Contains("angular") || message.Any(c => c is >= '\u0600' and <= '\u06ff')) return Call("search_projects", new { searchText = (string?)null, technology = Technology(message), page = 1, pageSize = 5 });
        if ((lower.Contains("that") || lower.Contains("which one") || lower.Contains("second one")) && history.Count > 0) return Call("search_projects", new { searchText = (string?)null, technology = Technology(string.Join(' ', history.Select(x => x.Content))), page = 1, pageSize = 5 });
        return null;
    }
    private static bool IsUnsafe(string x) => new[] { "connection string", "password", "secret", "system prompt", "select ", "insert ", "update ", "delete ", "drop ", "admin", "unpublished", "private phone", "ignore all rules" }.Any(x.Contains);
    private static string? Technology(string x) => new[] { "Angular", ".NET", "SQL Server", "OutSystems", "EF Core" }.FirstOrDefault(t => x.Contains(t, StringComparison.OrdinalIgnoreCase));
    private static string? ExtractSlug(string x, string marker) { var i = x.IndexOf(marker, StringComparison.OrdinalIgnoreCase); if (i < 0) return null; return x[(i + marker.Length)..].Split([' ', '?', '#'], StringSplitOptions.RemoveEmptyEntries)[0].Trim('/').ToLowerInvariant(); }
}
