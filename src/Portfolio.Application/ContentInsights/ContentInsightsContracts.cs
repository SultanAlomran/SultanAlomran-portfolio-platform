using Portfolio.Application.Infographics;
using Portfolio.Domain.Enums;

namespace Portfolio.Application.ContentInsights;

public sealed record ContentInsightsFilter(
    string DateRange = "30d",
    Guid? CategoryId = null,
    string? Search = null);

public sealed record ContentInsightsGuideQuery(
    string DateRange = "30d",
    Guid? CategoryId = null,
    string? Search = null,
    string? SortBy = "views",
    string? SortDirection = "desc",
    int Page = 1,
    int PageSize = 10);

public sealed record ContentInsightsSummaryDto(
    int TotalViews,
    int DeduplicatedViews,
    int HelpfulCount,
    int NotHelpfulCount,
    decimal? HelpfulPercentage,
    int TotalRatings,
    decimal? AverageRating,
    decimal EngagementRate,
    IReadOnlyList<RatingDistributionDto> RatingDistribution,
    IReadOnlyList<NegativeFeedbackReasonInsightDto> NegativeFeedbackBreakdown,
    IReadOnlyList<ContentInsightTrendPointDto> Trend,
    IReadOnlyList<InfographicInsightCardDto> TopViewed,
    IReadOnlyList<InfographicInsightCardDto> TopHelpful,
    IReadOnlyList<InfographicInsightCardDto> HighestRated,
    IReadOnlyList<InfographicInsightCardDto> LowestRated,
    IReadOnlyList<InfographicInsightCardDto> MostEngaged,
    IReadOnlyList<ContentNeedsAttentionDto> NeedsAttention);

public sealed record InfographicInsightCardDto(
    Guid Id,
    string Title,
    string Slug,
    string CategoryName,
    int TotalViews,
    int DeduplicatedViews,
    decimal? HelpfulPercentage,
    int HelpfulCount,
    int NotHelpfulCount,
    decimal? AverageRating,
    int RatingCount,
    decimal EngagementRate,
    int? HealthScore,
    string HealthStatus);

public sealed record ContentNeedsAttentionDto(
    Guid InfographicId,
    string Title,
    string Slug,
    string CategoryName,
    int TotalViews,
    int DeduplicatedViews,
    decimal? HelpfulPercentage,
    int HelpfulCount,
    int NotHelpfulCount,
    decimal? AverageRating,
    int RatingCount,
    decimal EngagementRate,
    string PrimaryReason,
    IReadOnlyList<string> Flags,
    string HealthStatus);

public sealed record InfographicInsightDto(
    Guid Id,
    string Title,
    string Slug,
    string CategoryName,
    ContentStatus Status,
    DifficultyLevel DifficultyLevel,
    DateTime? PublishedAt,
    int TotalViews,
    int DeduplicatedViews,
    int HelpfulCount,
    int NotHelpfulCount,
    decimal? HelpfulPercentage,
    int TotalRatings,
    decimal? AverageRating,
    IReadOnlyList<RatingDistributionDto> RatingDistribution,
    IReadOnlyList<NegativeFeedbackCountDto> NegativeReasons,
    decimal EngagementRate,
    int? HealthScore,
    string HealthStatus,
    IReadOnlyList<ContentInsightTrendPointDto> Trend);

public sealed record NegativeFeedbackReasonInsightDto(
    NegativeFeedbackReason Reason,
    string ReasonLabel,
    int Count,
    decimal Percentage,
    IReadOnlyList<AffectedGuideSummaryDto> TopAffectedGuides);

public sealed record AffectedGuideSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    string CategoryName,
    int Count);

public sealed record ContentInsightTrendPointDto(
    string Date,
    int Views,
    int HelpfulVotes,
    int NotHelpfulVotes,
    int Ratings);

public sealed record RecordViewRequest(
    string? Slug);
