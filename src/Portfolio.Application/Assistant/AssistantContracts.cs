namespace Portfolio.Application.Assistant;

public sealed record AssistantMessageRequest(string Message, IReadOnlyList<AssistantHistoryMessage>? ConversationContext);
public sealed record AssistantHistoryMessage(string Role, string Content);
public sealed record AssistantSource(string Type, string Title, string Route, string? Summary = null);
public sealed record AssistantAction(string Type, string Label, string Route);
public sealed record AssistantMessageResponse(string Message, IReadOnlyList<AssistantSource> Sources, IReadOnlyList<AssistantAction> Actions);
public sealed class AiAssistantOptions
{
    public const string SectionName = "AiAssistant";
    public bool Enabled { get; set; }
    public string Provider { get; set; } = "Deterministic";
    public string Model { get; set; } = "local-grounded-v1";
    public int MaxUserMessageLength { get; set; } = 1_000;
    public int MaxHistoryMessages { get; set; } = 8;
    public int MaxToolRounds { get; set; } = 4;
    public int MaxOutputCharacters { get; set; } = 4_000;
    public int RequestTimeoutSeconds { get; set; } = 20;
}
public interface IAiAssistantClient { Task<AssistantMessageResponse> CompleteAsync(AssistantGrounding grounding, CancellationToken token); }
public sealed record AssistantGrounding(string Message, IReadOnlyList<AssistantHistoryMessage> History, IReadOnlyList<AssistantSource> Evidence, string ProfileContext);
public interface IPortfolioAssistantService { Task<AssistantMessageResponse> SendAsync(AssistantMessageRequest request, CancellationToken token); }
public sealed class AssistantUnavailableException : Exception;
