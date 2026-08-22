using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Infographics;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Infographics;

internal sealed class InfographicEngagementService(PortfolioDbContext db) : IInfographicEngagementService
{
    private const string EntityType = "Infographic";

    public async Task<InfographicEngagementDto?> GetBySlugAsync(
        string slug, string? visitorKeyHash, CancellationToken token)
    {
        var infographicId = await db.Infographics.AsNoTracking()
            .Where(x => x.Status == ContentStatus.Published && x.Slug == slug.ToLower())
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(token);
        return infographicId.HasValue
            ? await GetAsync(infographicId.Value, visitorKeyHash, token)
            : null;
    }

    public async Task<InfographicEngagementDto> SetHelpfulVoteAsync(
        Guid infographicId, string visitorKeyHash, SetHelpfulVoteRequest request, CancellationToken token)
    {
        await EnsurePublishedAsync(infographicId, token);
        if (request.IsHelpful && request.Reason.HasValue)
            throw Validation("reason", "Helpful votes cannot include a negative feedback reason.");
        if (request.Reason.HasValue && !Enum.IsDefined(request.Reason.Value))
            throw Validation("reason", "Select a valid improvement reason.");

        var vote = await db.UserHelpfulVotes.SingleOrDefaultAsync(x =>
            x.VisitorKeyHash == visitorKeyHash && x.EntityType == EntityType && x.EntityId == infographicId, token);
        if (vote is null)
        {
            vote = UserHelpfulVote.ForVisitor(visitorKeyHash, EntityType, infographicId, request.IsHelpful, request.Reason);
            db.UserHelpfulVotes.Add(vote);
        }
        else
        {
            vote.SetVote(request.IsHelpful, request.Reason);
        }

        await SaveWithDuplicateRecoveryAsync(async () =>
        {
            var existing = await db.UserHelpfulVotes.SingleAsync(x =>
                x.VisitorKeyHash == visitorKeyHash && x.EntityType == EntityType && x.EntityId == infographicId, token);
            existing.SetVote(request.IsHelpful, request.Reason);
        }, token);
        return await GetAsync(infographicId, visitorKeyHash, token);
    }

    public async Task<InfographicEngagementDto> SetRatingAsync(
        Guid infographicId, string visitorKeyHash, SetInfographicRatingRequest request, CancellationToken token)
    {
        await EnsurePublishedAsync(infographicId, token);
        if (request.Rating is < 1 or > 5)
            throw Validation("rating", "Rating must be between 1 and 5.");

        var rating = await db.UserRatings.SingleOrDefaultAsync(x =>
            x.VisitorKeyHash == visitorKeyHash && x.EntityType == EntityType && x.EntityId == infographicId, token);
        if (rating is null)
        {
            rating = UserRating.ForVisitor(visitorKeyHash, EntityType, infographicId, request.Rating);
            db.UserRatings.Add(rating);
        }
        else
        {
            rating.SetRating(request.Rating);
        }

        await SaveWithDuplicateRecoveryAsync(async () =>
        {
            var existing = await db.UserRatings.SingleAsync(x =>
                x.VisitorKeyHash == visitorKeyHash && x.EntityType == EntityType && x.EntityId == infographicId, token);
            existing.SetRating(request.Rating);
        }, token);
        return await GetAsync(infographicId, visitorKeyHash, token);
    }

    private async Task<InfographicEngagementDto> GetAsync(
        Guid infographicId, string? visitorKeyHash, CancellationToken token)
    {
        var voteCounts = await db.UserHelpfulVotes.AsNoTracking()
            .Where(x => x.EntityType == EntityType && x.EntityId == infographicId)
            .GroupBy(x => x.IsHelpful)
            .Select(group => new { IsHelpful = group.Key, Count = group.Count() })
            .ToListAsync(token);
        var helpfulCount = voteCounts.FirstOrDefault(x => x.IsHelpful)?.Count ?? 0;
        var notHelpfulCount = voteCounts.FirstOrDefault(x => !x.IsHelpful)?.Count ?? 0;
        var responseCount = helpfulCount + notHelpfulCount;
        decimal? helpfulPercentage = responseCount == 0
            ? null
            : Math.Round(helpfulCount * 100m / responseCount, 1);

        var ratingSummary = await db.UserRatings.AsNoTracking()
            .Where(x => x.EntityType == EntityType && x.EntityId == infographicId)
            .GroupBy(_ => 1)
            .Select(group => new { Count = group.Count(), Average = group.Average(x => (decimal)x.Rating) })
            .SingleOrDefaultAsync(token);
        var ratingCounts = await db.UserRatings.AsNoTracking()
            .Where(x => x.EntityType == EntityType && x.EntityId == infographicId)
            .GroupBy(x => x.Rating)
            .Select(group => new RatingDistributionDto(group.Key, group.Count()))
            .ToListAsync(token);
        var distribution = Enumerable.Range(1, 5).Reverse()
            .Select(value => new RatingDistributionDto((byte)value,
                ratingCounts.FirstOrDefault(item => item.Rating == value)?.Count ?? 0))
            .ToList();

        var reasonCounts = await db.UserHelpfulVotes.AsNoTracking()
            .Where(x => x.EntityType == EntityType && x.EntityId == infographicId &&
                !x.IsHelpful && x.NegativeFeedbackReason.HasValue)
            .GroupBy(x => x.NegativeFeedbackReason!.Value)
            .Select(group => new { Reason = group.Key, Count = group.Count() })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Reason)
            .ToListAsync(token);
        var reasons = reasonCounts
            .Select(x => new NegativeFeedbackCountDto(x.Reason, x.Count))
            .ToList();

        bool? visitorVote = null;
        NegativeFeedbackReason? visitorReason = null;
        byte? visitorRating = null;
        if (!string.IsNullOrWhiteSpace(visitorKeyHash))
        {
            var ownVote = await db.UserHelpfulVotes.AsNoTracking()
                .Where(x => x.VisitorKeyHash == visitorKeyHash && x.EntityType == EntityType && x.EntityId == infographicId)
                .Select(x => new { x.IsHelpful, x.NegativeFeedbackReason })
                .SingleOrDefaultAsync(token);
            visitorVote = ownVote?.IsHelpful;
            visitorReason = ownVote?.NegativeFeedbackReason;
            visitorRating = await db.UserRatings.AsNoTracking()
                .Where(x => x.VisitorKeyHash == visitorKeyHash && x.EntityType == EntityType && x.EntityId == infographicId)
                .Select(x => (byte?)x.Rating)
                .SingleOrDefaultAsync(token);
        }

        return new(helpfulCount, notHelpfulCount, helpfulPercentage,
            ratingSummary is null ? null : Math.Round(ratingSummary.Average, 2),
            ratingSummary?.Count ?? 0, distribution, reasons, visitorVote, visitorReason, visitorRating);
    }

    private async Task EnsurePublishedAsync(Guid infographicId, CancellationToken token)
    {
        if (!await db.Infographics.AsNoTracking().AnyAsync(
                x => x.Id == infographicId && x.Status == ContentStatus.Published, token))
            throw new InfographicNotFoundException(infographicId);
    }

    private async Task SaveWithDuplicateRecoveryAsync(Func<Task> recover, CancellationToken token)
    {
        try
        {
            await db.SaveChangesAsync(token);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            foreach (var entry in exception.Entries)
                entry.State = EntityState.Detached;
            await recover();
            await db.SaveChangesAsync(token);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException { Number: 2601 or 2627 };

    private static InfographicValidationException Validation(string key, string message) =>
        new(new Dictionary<string, string[]> { [key] = [message] });
}
