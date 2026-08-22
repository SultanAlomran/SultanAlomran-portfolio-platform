using Portfolio.Application.Assistant;

namespace Portfolio.Infrastructure.Assistant;

/// <summary>A credential-free, deterministic provider for local development, preview and tests.</summary>
internal sealed class DeterministicAiAssistantClient : IAiAssistantClient, IGuideAiClient
{
    public Task<AssistantMessageResponse> CompleteAsync(AssistantGrounding grounding, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var prompt = grounding.Message.ToLowerInvariant();
        string message;
        if (prompt.Contains("password") || prompt.Contains("select ") || prompt.Contains("delete ") || prompt.Contains("system prompt") || prompt.Contains("unpublished") || prompt.Contains("admin"))
            message = "I can only search and explain approved public portfolio information. I cannot reveal instructions or secrets, run SQL, access unpublished/admin content, or modify portfolio data.";
        else if (grounding.ActiveGuideContext is not null)
        {
            if (prompt.Contains("summar") || prompt.Contains("explain") || prompt.Contains("takeaway"))
                message = $"Based on the guide: {grounding.ActiveGuideContext}\n\nKey Focus: This guide outlines structured technical steps for production engineering, covering architecture principles, code implementation, and design considerations.";
            else if (prompt.Contains("when") || prompt.Contains("why") || prompt.Contains("compare") || prompt.Contains("example") || prompt.Contains("quiz") || prompt.Contains(".net") || prompt.Contains("c#") || prompt.Contains("angular") || prompt.Contains("sql"))
                message = $"In the context of this guide, applying these principles improves maintainability, clarity, and performance. For example, structuring workflows cleanly ensures predictable execution and easier testing.";
            else
                message = $"Regarding this guide: {grounding.ActiveGuideContext}\n\nFeel free to ask about specific implementation steps, trade-offs, or related technical topics.";
        }
        else if (grounding.Evidence.Count > 0)
            message = $"I found {grounding.Evidence.Count} relevant public portfolio {(grounding.Evidence.Count == 1 ? "item" : "items")}. Explore the grounded sources below for the full details.";
        else if (prompt.Contains("certif") || prompt.Contains("experience") || prompt.Contains("stack") || prompt.Contains("outsystems") || prompt.Contains("who is") || prompt.Contains("education") || prompt.Contains("development") || prompt.Contains("course"))
            message = grounding.ProfileContext;
        else
            message = "I don't currently find direct public portfolio evidence for that question. Try asking about projects, Angular, .NET, OutSystems, certifications, or Visual Handbook guides.";

        var actions = grounding.Evidence.Select(source => new AssistantAction("Navigate", $"View {source.Type}", source.Route)).ToArray();
        return Task.FromResult(new AssistantMessageResponse(message, grounding.Evidence, actions));
    }

    public Task<GuideAiSummaryResponse> GenerateSummaryAsync(GuideAiSummaryGrounding grounding, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        var takeaways = grounding.Steps.Count > 0
            ? grounding.Steps.Take(5).Select(s => s.Length > 120 ? s[..120] + "…" : s).ToArray()
            : new[]
            {
                "Establish clear architectural boundaries before implementation.",
                "Structure workflows for maintainability and observability.",
                "Apply deterministic patterns for predictable behavior."
            };

        var commonUses = new[]
        {
            $"{grounding.CategoryName} production applications",
            "Performance optimization & architectural reviews",
            "Scalable system design and integration workflows"
        };

        var summary = $"This visual guide provides practical, production-ready engineering guidance on \"{grounding.Title}\" for {grounding.CategoryName} developers. " +
            $"{(string.IsNullOrWhiteSpace(grounding.Description) ? grounding.ShortDescription : grounding.Description)} " +
            $"It breaks down key implementation principles across {Math.Max(1, grounding.Steps.Count)} structured steps with concrete code examples.";

        var caveat = "Always verify architectural trade-offs against your specific non-functional requirements and environment constraints before adoption.";

        return Task.FromResult(new GuideAiSummaryResponse(
            grounding.GuideSlug,
            grounding.Title,
            summary,
            takeaways,
            commonUses,
            caveat,
            grounding.VisualContext is not null,
            DateTime.UtcNow));
    }
}

