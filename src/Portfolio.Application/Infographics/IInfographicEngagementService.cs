namespace Portfolio.Application.Infographics;

public interface IInfographicEngagementService
{
    Task<InfographicEngagementDto?> GetBySlugAsync(string slug, string? visitorKeyHash, CancellationToken token);
    Task<InfographicEngagementDto> SetHelpfulVoteAsync(Guid infographicId, string visitorKeyHash,
        SetHelpfulVoteRequest request, CancellationToken token);
    Task<InfographicEngagementDto> SetRatingAsync(Guid infographicId, string visitorKeyHash,
        SetInfographicRatingRequest request, CancellationToken token);
}
