using Portfolio.Domain.Enums;

namespace Portfolio.Application.Infographics;

public sealed record RatingDistributionDto(byte Rating, int Count);
public sealed record NegativeFeedbackCountDto(NegativeFeedbackReason Reason, int Count);

public sealed record InfographicEngagementDto(
    int HelpfulCount,
    int NotHelpfulCount,
    decimal? HelpfulPercentage,
    decimal? AverageRating,
    int RatingCount,
    IReadOnlyList<RatingDistributionDto> RatingDistribution,
    IReadOnlyList<NegativeFeedbackCountDto> NegativeFeedback,
    bool? VisitorHelpfulVote,
    NegativeFeedbackReason? VisitorNegativeFeedbackReason,
    byte? VisitorRating);

public sealed record SetHelpfulVoteRequest(bool IsHelpful, NegativeFeedbackReason? Reason);
public sealed record SetInfographicRatingRequest(byte Rating);
