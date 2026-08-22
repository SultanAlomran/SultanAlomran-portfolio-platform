using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Infographics;
using Portfolio.Application.Media;

namespace Portfolio.Application.Assistant;

public sealed class GuideAiService(
    IInfographicsService infographicsService,
    IMediaStorage mediaStorage,
    IGuideAiClient client,
    IMemoryCache cache,
    IOptions<AiAssistantOptions> options,
    ILogger<GuideAiService> logger) : IGuideAiService
{
    public async Task<GuideAiSummaryResponse> GenerateSummaryAsync(string slug, CancellationToken token)
    {
        var settings = options.Value;
        if (!settings.Enabled) throw new AssistantUnavailableException();

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Guide slug is required.");

        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var guide = await infographicsService.GetPublicBySlugAsync(normalizedSlug, token);
        if (guide is null)
            throw new KeyNotFoundException($"Published guide with slug '{slug}' was not found.");

        var versionTimestamp = guide.UpdatedAt?.Ticks ?? guide.PublishedAt?.Ticks ?? guide.CreatedAt.Ticks;
        var cacheKey = $"guide-ai-summary:{guide.Id}:{versionTimestamp}:{settings.Model}:{settings.Provider}";

        if (cache.TryGetValue(cacheKey, out GuideAiSummaryResponse? cached) && cached is not null)
        {
            logger.LogInformation("Returning cached AI summary for guide {Slug}", guide.Slug);
            return cached;
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(settings.RequestTimeoutSeconds, 1, 120)));

        try
        {
            GuideVisualContext? visualContext = null;
            if (!string.IsNullOrWhiteSpace(guide.InfographicUrl))
            {
                var imageBytes = await mediaStorage.ReadBytesAsync(guide.InfographicUrl, timeout.Token);
                if (imageBytes is { Length: > 0 })
                {
                    var mimeType = DetectMimeType(guide.InfographicUrl);
                    visualContext = new GuideVisualContext(mimeType, imageBytes);
                }
            }

            var steps = guide.Steps
                .OrderBy(s => s.DisplayOrder).ThenBy(s => s.StepNumber)
                .Select(s => $"Step {s.StepNumber}: {s.Title}. {s.Content}".Trim())
                .ToList();

            var codeSnippets = guide.CodeExamples
                .OrderBy(c => c.DisplayOrder)
                .Select(c => $"[{c.Language}] {c.Title}:\n{c.Code}".Trim())
                .ToList();

            var tags = guide.Tags.Select(t => t.Name).ToList();

            var grounding = new GuideAiSummaryGrounding(
                guide.Slug,
                guide.Title,
                guide.ShortDescription,
                guide.Description,
                guide.Category.Name,
                difficultyLabel(guide.DifficultyLevel),
                tags,
                steps,
                codeSnippets,
                visualContext);

            var summaryResponse = await client.GenerateSummaryAsync(grounding, timeout.Token);

            var outputLimit = Math.Clamp(settings.MaxOutputCharacters, 100, 20_000);
            var sanitizedSummary = summaryResponse.Summary.Length <= outputLimit
                ? summaryResponse.Summary
                : summaryResponse.Summary[..outputLimit];

            var result = summaryResponse with
            {
                Summary = sanitizedSummary,
                KeyTakeaways = summaryResponse.KeyTakeaways.Take(10).ToArray(),
                CommonUses = summaryResponse.CommonUses.Take(10).ToArray()
            };

            cache.Set(cacheKey, result, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
                Size = 1
            });

            logger.LogInformation("Generated and cached AI summary for guide {Slug} (VisualGrounded: {VisualGrounded})",
                guide.Slug, result.IsVisualGrounded);

            return result;
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            throw new AssistantProviderException("The AI summary provider timed out.");
        }
    }

    private static string DetectMimeType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            _ => "image/png"
        };
    }

    private static string difficultyLabel(Domain.Enums.DifficultyLevel level) => level switch
    {
        Domain.Enums.DifficultyLevel.Beginner => "Beginner",
        Domain.Enums.DifficultyLevel.Intermediate => "Intermediate",
        Domain.Enums.DifficultyLevel.Advanced => "Advanced",
        _ => "Intermediate"
    };
}
