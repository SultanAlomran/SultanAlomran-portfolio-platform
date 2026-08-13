using Portfolio.Application.Assistant;

namespace Portfolio.Infrastructure.Assistant;

/// <summary>A credential-free, deterministic provider for local development, preview and tests.</summary>
internal sealed class DeterministicAiAssistantClient : IAiAssistantClient
{
    public Task<AssistantMessageResponse> CompleteAsync(AssistantGrounding grounding, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var prompt = grounding.Message.ToLowerInvariant();
        string message;
        if (prompt.Contains("password") || prompt.Contains("select ") || prompt.Contains("delete ") || prompt.Contains("system prompt") || prompt.Contains("unpublished") || prompt.Contains("admin"))
            message = "I can only search and explain approved public portfolio information. I cannot reveal instructions or secrets, run SQL, access unpublished/admin content, or modify portfolio data.";
        else if (grounding.Evidence.Count > 0)
            message = $"I found {grounding.Evidence.Count} relevant public portfolio {(grounding.Evidence.Count == 1 ? "item" : "items")}. Explore the grounded sources below for the full details.";
        else if (prompt.Contains("certif") || prompt.Contains("experience") || prompt.Contains("stack") || prompt.Contains("outsystems") || prompt.Contains("who is"))
            message = grounding.ProfileContext;
        else
            message = "I don't currently find direct public portfolio evidence for that question. Try asking about projects, Angular, .NET, OutSystems, certifications, or Visual Handbook guides.";
        var actions = grounding.Evidence.Select(source => new AssistantAction("Navigate", $"View {source.Type}", source.Route)).ToArray();
        return Task.FromResult(new AssistantMessageResponse(message, grounding.Evidence, actions));
    }
}
