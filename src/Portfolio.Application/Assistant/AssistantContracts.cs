using System.Text.Json;

namespace Portfolio.Application.Assistant;

public sealed record AssistantMessageRequest(string Message, IReadOnlyList<AssistantHistoryMessage>? ConversationContext);
public sealed record AssistantHistoryMessage(string Role, string Content);
public sealed record AssistantSource(string Type, string Title, string Route, string? Summary = null);
public sealed record AssistantAction(string Type, string Label, string Route);
public sealed record AssistantMessageResponse(string Message, IReadOnlyList<AssistantSource> Sources,
    IReadOnlyList<AssistantAction> Actions, IReadOnlyList<string>? SuggestedFollowUps = null, string? Language = null);

public sealed class AiAssistantOptions
{
    public const string SectionName = "AiAssistant";
    public bool Enabled { get; set; }
    public string Provider { get; set; } = "Deterministic";
    public string Model { get; set; } = "local-grounded-v2";
    public int MaxUserMessageLength { get; set; } = 1_000;
    public int MaxHistoryMessages { get; set; } = 8;
    public int MaxToolRounds { get; set; } = 4;
    public int MaxOutputCharacters { get; set; } = 4_000;
    public int MaxOutputTokens { get; set; } = 800;
    public int RequestTimeoutSeconds { get; set; } = 20;
    public double Temperature { get; set; } = 0.2;
    public bool RealProviderEnabled { get; set; }
    public int RateLimitPermitCount { get; set; } = 10;
    public int RateLimitWindowSeconds { get; set; } = 60;
}

public sealed record AssistantToolDefinition(string Name, string Description, JsonElement Parameters);
public sealed record AssistantToolCall(string Id, string Name, JsonElement Arguments);
public sealed record AssistantToolResult(string CallId, string Name, JsonElement Output, IReadOnlyList<AssistantSource> Sources, IReadOnlyList<AssistantAction>? Actions = null);
public sealed record AssistantProviderUsage(int? InputTokens, int? OutputTokens);
public sealed record AssistantProviderTurn(string? Message, IReadOnlyList<AssistantToolCall> ToolCalls,
    IReadOnlyList<string>? SuggestedFollowUps = null, string? Language = null, AssistantProviderUsage? Usage = null);
public sealed record AssistantProviderRequest(string SystemInstruction, string Message,
    IReadOnlyList<AssistantHistoryMessage> History, IReadOnlyList<AssistantToolDefinition> Tools,
    IReadOnlyList<AssistantToolResult> ToolResults, int MaxOutputTokens, double Temperature);

public interface IAiAssistantClient
{
    string ProviderName { get; }
    string ModelName { get; }
    Task<AssistantProviderTurn> CompleteAsync(AssistantProviderRequest request, CancellationToken token);
}

public interface IPortfolioAssistantService
{
    Task<AssistantMessageResponse> SendAsync(AssistantMessageRequest request, CancellationToken token);
}

public interface IAssistantTools
{
    IReadOnlyList<AssistantToolDefinition> Definitions { get; }
    Task<AssistantToolResult> ExecuteAsync(AssistantToolCall call, CancellationToken token);
}

public sealed class AssistantUnavailableException : Exception;
public sealed class AssistantProviderException(string message, Exception? inner = null) : Exception(message, inner);
public sealed class AssistantToolException(string message) : Exception(message);
