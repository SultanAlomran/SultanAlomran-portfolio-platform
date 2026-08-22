namespace Portfolio.Application.Assistant;

public sealed record AssistantMessageRequest(string Message, IReadOnlyList<AssistantHistoryMessage>? ConversationContext, string? GuideSlug = null);
public sealed record AssistantHistoryMessage(string Role, string Content);
public sealed record AssistantSource(string Type, string Title, string Route, string? Summary = null);
public sealed record AssistantAction(string Type, string Label, string Route);
public sealed record AssistantMessageResponse(string Message, IReadOnlyList<AssistantSource> Sources, IReadOnlyList<AssistantAction> Actions);

public sealed record GuideVisualContext(string MimeType, byte[] Data);

public sealed record GuideAiSummaryResponse(
    string GuideSlug,
    string Title,
    string Summary,
    IReadOnlyList<string> KeyTakeaways,
    IReadOnlyList<string> CommonUses,
    string? Caveat,
    bool IsVisualGrounded,
    DateTime GeneratedAt);

public sealed record GuideAiSummaryGrounding(
    string GuideSlug,
    string Title,
    string ShortDescription,
    string? Description,
    string CategoryName,
    string Difficulty,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> Steps,
    IReadOnlyList<string> CodeSnippets,
    GuideVisualContext? VisualContext);

public sealed class AiAssistantOptions
{
    public const string SectionName = "AiAssistant";
    public bool Enabled { get; set; }
    public string Provider { get; set; } = "Deterministic";
    public string Model { get; set; } = "gpt-5.6";
    public string? ApiKey { get; set; }
    public string? Endpoint { get; set; }
    public int MaxUserMessageLength { get; set; } = 1_000;
    public int MaxHistoryMessages { get; set; } = 8;
    public int MaxToolRounds { get; set; } = 4;
    public int MaxOutputCharacters { get; set; } = 4_000;
    public int RequestTimeoutSeconds { get; set; } = 25;
}

public interface IAiAssistantClient
{
    Task<AssistantMessageResponse> CompleteAsync(AssistantGrounding grounding, CancellationToken token);
}

public interface IGuideAiClient
{
    Task<GuideAiSummaryResponse> GenerateSummaryAsync(GuideAiSummaryGrounding grounding, CancellationToken token);
}

public sealed record AssistantGrounding(
    string Message,
    IReadOnlyList<AssistantHistoryMessage> History,
    IReadOnlyList<AssistantSource> Evidence,
    string ProfileContext,
    string? ActiveGuideContext = null,
    GuideVisualContext? GuideVisualContext = null);

public interface IPortfolioAssistantService
{
    Task<AssistantMessageResponse> SendAsync(AssistantMessageRequest request, CancellationToken token);
}

public interface IGuideAiService
{
    Task<GuideAiSummaryResponse> GenerateSummaryAsync(string slug, CancellationToken token);
}

public interface IAssistantTools
{
    Task<IReadOnlyList<AssistantSource>> SearchProjectsAsync(string? technology, CancellationToken token);
    Task<AssistantSource?> GetProjectDetailsAsync(string slug, CancellationToken token);
    Task<IReadOnlyList<AssistantSource>> SearchInfographicsAsync(string? search, CancellationToken token);
    Task<AssistantSource?> GetInfographicDetailsAsync(string slug, CancellationToken token);
}

public sealed class AssistantUnavailableException : Exception;
public sealed class AssistantProviderException(string message) : Exception(message);

