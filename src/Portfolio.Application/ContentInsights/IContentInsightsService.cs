using Portfolio.Application.Infographics;

namespace Portfolio.Application.ContentInsights;

public interface IContentInsightsService
{
    Task<ContentInsightsSummaryDto> GetSummaryAsync(ContentInsightsFilter filter, CancellationToken token = default);
    Task<InfographicPagedResult<InfographicInsightDto>> GetGuidesAsync(ContentInsightsGuideQuery query, CancellationToken token = default);
    Task<InfographicInsightDto?> GetGuideDetailsAsync(Guid id, ContentInsightsFilter filter, CancellationToken token = default);
    Task<bool> RecordViewAsync(string slug, string visitorKeyHash, CancellationToken token = default);
}
