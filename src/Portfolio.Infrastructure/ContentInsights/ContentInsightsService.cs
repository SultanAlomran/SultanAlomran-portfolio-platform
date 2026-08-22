using Microsoft.EntityFrameworkCore;
using Portfolio.Application.ContentInsights;
using Portfolio.Application.Infographics;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.ContentInsights;

internal sealed class ContentInsightsService(PortfolioDbContext db) : IContentInsightsService
{
    private const string EntityType = "Infographic";

    public async Task<bool> RecordViewAsync(string slug, string visitorKeyHash, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(slug) || string.IsNullOrWhiteSpace(visitorKeyHash))
            return false;

        var infographicId = await db.Infographics.AsNoTracking()
            .Where(x => x.Status == ContentStatus.Published && x.Slug == slug.ToLower())
            .Select(x => (Guid?)x.Id)
            .SingleOrDefaultAsync(token);

        if (!infographicId.HasValue) return false;

        var deduplicationCutoff = DateTime.UtcNow.AddMinutes(-30);
        var recentViewExists = await db.InfographicViews.AsNoTracking()
            .AnyAsync(x => x.InfographicId == infographicId.Value &&
                           x.VisitorKeyHash == visitorKeyHash &&
                           x.CreatedAt >= deduplicationCutoff, token);

        if (recentViewExists) return true;

        var view = InfographicView.Create(infographicId.Value, visitorKeyHash);
        db.InfographicViews.Add(view);
        await db.SaveChangesAsync(token);
        return true;
    }

    public async Task<ContentInsightsSummaryDto> GetSummaryAsync(ContentInsightsFilter filter, CancellationToken token = default)
    {
        var fromDate = ResolveFilterDate(filter.DateRange);

        // 1. Fetch published infographics matching category/search
        var infographicsQuery = db.Infographics.AsNoTracking()
            .Where(x => x.Status == ContentStatus.Published);

        if (filter.CategoryId.HasValue)
            infographicsQuery = infographicsQuery.Where(x => x.CategoryId == filter.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            infographicsQuery = infographicsQuery.Where(x => x.Title.ToLower().Contains(search) || x.Slug.Contains(search));
        }

        var publishedGuides = await infographicsQuery
            .Select(x => new { x.Id, x.Title, x.Slug, CategoryName = x.Category.Name })
            .ToListAsync(token);

        var guideIds = publishedGuides.Select(x => x.Id).ToHashSet();

        // 2. Query views in range for these guides
        var viewsQuery = db.InfographicViews.AsNoTracking()
            .Where(x => guideIds.Contains(x.InfographicId) && x.CreatedAt >= fromDate);

        var totalViews = await viewsQuery.CountAsync(token);
        var deduplicatedViews = await viewsQuery
            .Select(x => new { x.InfographicId, x.VisitorKeyHash })
            .Distinct()
            .CountAsync(token);

        var guideViewsData = await viewsQuery
            .GroupBy(x => x.InfographicId)
            .Select(g => new
            {
                InfographicId = g.Key,
                TotalViews = g.Count(),
                DeduplicatedViews = g.Select(v => v.VisitorKeyHash).Distinct().Count(),
                VisitorHashes = g.Select(v => v.VisitorKeyHash).Distinct().ToList()
            })
            .ToListAsync(token);

        // 3. Query helpful votes in range for these guides
        var votesQuery = db.UserHelpfulVotes.AsNoTracking()
            .Where(x => x.EntityType == EntityType && guideIds.Contains(x.EntityId) && x.CreatedAt >= fromDate);

        var helpfulCount = await votesQuery.CountAsync(x => x.IsHelpful, token);
        var notHelpfulCount = await votesQuery.CountAsync(x => !x.IsHelpful, token);
        var totalVotes = helpfulCount + notHelpfulCount;
        decimal? helpfulPercentage = totalVotes == 0
            ? null
            : Math.Round(helpfulCount * 100m / totalVotes, 1);

        var guideVotesData = await votesQuery
            .GroupBy(x => x.EntityId)
            .Select(g => new
            {
                InfographicId = g.Key,
                Helpful = g.Count(v => v.IsHelpful),
                NotHelpful = g.Count(v => !v.IsHelpful),
                Voters = g.Select(v => v.VisitorKeyHash).Where(h => h != null).Distinct().ToList()
            })
            .ToListAsync(token);

        // 4. Query ratings in range for these guides
        var ratingsQuery = db.UserRatings.AsNoTracking()
            .Where(x => x.EntityType == EntityType && guideIds.Contains(x.EntityId) && x.CreatedAt >= fromDate);

        var ratingItems = await ratingsQuery.Select(x => x.Rating).ToListAsync(token);
        var totalRatings = ratingItems.Count;
        var averageRating = totalRatings == 0
            ? (decimal?)null
            : Math.Round((decimal)ratingItems.Average(r => (double)r), 2);

        var ratingDistributionCounts = await ratingsQuery
            .GroupBy(x => x.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync(token);

        var ratingDistribution = Enumerable.Range(1, 5).Reverse()
            .Select(value => new RatingDistributionDto(
                (byte)value,
                ratingDistributionCounts.FirstOrDefault(x => x.Rating == value)?.Count ?? 0))
            .ToList();

        var guideRatingsData = await ratingsQuery
            .GroupBy(x => x.EntityId)
            .Select(g => new
            {
                InfographicId = g.Key,
                Count = g.Count(),
                Average = (decimal)g.Average(r => (double)r.Rating),
                Raters = g.Select(r => r.VisitorKeyHash).Where(h => h != null).Distinct().ToList()
            })
            .ToListAsync(token);

        // 5. Global Engagement Rate
        var allEngagedVisitors = await votesQuery
            .Where(v => v.VisitorKeyHash != null)
            .Select(v => v.VisitorKeyHash!)
            .Union(ratingsQuery.Where(r => r.VisitorKeyHash != null).Select(r => r.VisitorKeyHash!))
            .Distinct()
            .CountAsync(token);

        var globalEngagementRate = deduplicatedViews == 0
            ? 0m
            : Math.Min(100m, Math.Round(allEngagedVisitors * 100m / deduplicatedViews, 1));

        // 6. Negative feedback breakdown
        var rawNegativeVotes = await db.UserHelpfulVotes.AsNoTracking()
            .Where(x => x.EntityType == EntityType && guideIds.Contains(x.EntityId) &&
                        !x.IsHelpful && x.NegativeFeedbackReason.HasValue && x.CreatedAt >= fromDate)
            .Select(x => new { Reason = x.NegativeFeedbackReason!.Value, InfographicId = x.EntityId })
            .ToListAsync(token);

        var totalNegativeWithReason = rawNegativeVotes.Count;
        var negativeFeedbackBreakdown = Enum.GetValues<NegativeFeedbackReason>()
            .Select(reason =>
            {
                var matchingVotes = rawNegativeVotes.Where(x => x.Reason == reason).ToList();
                var count = matchingVotes.Count;
                var pct = totalNegativeWithReason == 0
                    ? 0m
                    : Math.Round(count * 100m / totalNegativeWithReason, 1);

                var affected = matchingVotes
                    .GroupBy(x => x.InfographicId)
                    .Select(ag => new { InfographicId = ag.Key, Count = ag.Count() })
                    .OrderByDescending(ag => ag.Count)
                    .Take(3)
                    .Select(ag =>
                    {
                        var guide = publishedGuides.FirstOrDefault(g => g.Id == ag.InfographicId);
                        return new AffectedGuideSummaryDto(
                            ag.InfographicId,
                            guide?.Title ?? "Guide",
                            guide?.Slug ?? "",
                            guide?.CategoryName ?? "",
                            ag.Count);
                    }).ToList();

                return new NegativeFeedbackReasonInsightDto(
                    reason,
                    GetReasonLabel(reason),
                    count,
                    pct,
                    affected);
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.ReasonLabel)
            .ToList();

        // 7. Trend data (daily points)
        var trendPoints = await BuildTrendAsync(guideIds, fromDate, token);

        // 8. Build per-guide card summaries for rankings & needs attention
        var guideCards = publishedGuides.Select(guide =>
        {
            var vData = guideViewsData.FirstOrDefault(x => x.InfographicId == guide.Id);
            var fData = guideVotesData.FirstOrDefault(x => x.InfographicId == guide.Id);
            var rData = guideRatingsData.FirstOrDefault(x => x.InfographicId == guide.Id);

            var gTotalViews = vData?.TotalViews ?? 0;
            var gDedupViews = vData?.DeduplicatedViews ?? 0;
            var gHelpful = fData?.Helpful ?? 0;
            var gNotHelpful = fData?.NotHelpful ?? 0;
            var gTotalVotes = gHelpful + gNotHelpful;
            decimal? gHelpfulPct = gTotalVotes == 0
                ? null
                : Math.Round(gHelpful * 100m / gTotalVotes, 1);

            var gRatingCount = rData?.Count ?? 0;
            decimal? gAvgRating = gRatingCount == 0
                ? null
                : Math.Round(rData!.Average, 2);

            var engagedHashes = (fData?.Voters.Where(h => h != null).Select(h => h!) ?? Enumerable.Empty<string>())
                .Union(rData?.Raters.Where(h => h != null).Select(h => h!) ?? Enumerable.Empty<string>())
                .Distinct()
                .Count();

            var gEngRate = gDedupViews == 0
                ? 0m
                : Math.Min(100m, Math.Round(engagedHashes * 100m / gDedupViews, 1));

            var (score, status) = CalculateHealth(gHelpful, gNotHelpful, gAvgRating, gRatingCount, gDedupViews, gEngRate);

            return new InfographicInsightCardDto(
                guide.Id,
                guide.Title,
                guide.Slug,
                guide.CategoryName,
                gTotalViews,
                gDedupViews,
                gHelpfulPct,
                gHelpful,
                gNotHelpful,
                gAvgRating,
                gRatingCount,
                gEngRate,
                score,
                status);
        }).ToList();

        // 9. Top sections
        var topViewed = guideCards
            .Where(x => x.TotalViews > 0)
            .OrderByDescending(x => x.TotalViews)
            .Take(5)
            .ToList();

        var topHelpful = guideCards
            .Where(x => (x.HelpfulCount + x.NotHelpfulCount) >= 1 && x.HelpfulPercentage.HasValue)
            .OrderByDescending(x => x.HelpfulPercentage)
            .ThenByDescending(x => x.HelpfulCount)
            .Take(5)
            .ToList();

        var highestRated = guideCards
            .Where(x => x.RatingCount >= 1 && x.AverageRating.HasValue)
            .OrderByDescending(x => x.AverageRating)
            .ThenByDescending(x => x.RatingCount)
            .Take(5)
            .ToList();

        var lowestRated = guideCards
            .Where(x => x.RatingCount >= 1 && x.AverageRating.HasValue)
            .OrderBy(x => x.AverageRating)
            .ThenByDescending(x => x.RatingCount)
            .Take(5)
            .ToList();

        var mostEngaged = guideCards
            .Where(x => x.DeduplicatedViews >= 3 && x.EngagementRate > 0)
            .OrderByDescending(x => x.EngagementRate)
            .Take(5)
            .ToList();

        // 10. Needs Attention list
        var needsAttention = new List<ContentNeedsAttentionDto>();
        foreach (var card in guideCards)
        {
            var flags = new List<string>();
            var totalResponses = card.HelpfulCount + card.NotHelpfulCount;

            if (totalResponses >= 3 && card.HelpfulPercentage.HasValue && card.HelpfulPercentage.Value < 70m)
                flags.Add($"Low helpfulness ratio ({card.HelpfulPercentage.Value}%)");

            if (card.RatingCount >= 3 && card.AverageRating.HasValue && card.AverageRating.Value < 3.8m)
                flags.Add($"Low average rating ({card.AverageRating.Value} / 5)");

            if (card.NotHelpfulCount >= 2)
                flags.Add($"Negative feedback volume ({card.NotHelpfulCount} downvotes)");

            if (card.DeduplicatedViews >= 15 && card.EngagementRate < 5m)
                flags.Add($"High views ({card.DeduplicatedViews}) with low engagement ({card.EngagementRate}%)");

            var topReasonMatch = negativeFeedbackBreakdown
                .FirstOrDefault(r => r.TopAffectedGuides.Any(ag => ag.Id == card.Id));
            if (topReasonMatch != null && card.NotHelpfulCount >= 2)
                flags.Add($"Top issue: {topReasonMatch.ReasonLabel}");

            if (flags.Count > 0 || card.HealthStatus is "Needs Attention" or "Critical")
            {
                var primaryReason = flags.FirstOrDefault() ??
                                    (card.HealthStatus == "Critical" ? "Critical content health score" : "Requires quality review");

                needsAttention.Add(new ContentNeedsAttentionDto(
                    card.Id,
                    card.Title,
                    card.Slug,
                    card.CategoryName,
                    card.TotalViews,
                    card.DeduplicatedViews,
                    card.HelpfulPercentage,
                    card.HelpfulCount,
                    card.NotHelpfulCount,
                    card.AverageRating,
                    card.RatingCount,
                    card.EngagementRate,
                    primaryReason,
                    flags,
                    card.HealthStatus == "Critical" ? "Critical" : "Needs Attention"));
            }
        }

        needsAttention = needsAttention
            .OrderBy(x => x.HealthStatus == "Critical" ? 0 : 1)
            .ThenByDescending(x => x.Flags.Count)
            .ThenByDescending(x => x.NotHelpfulCount)
            .Take(10)
            .ToList();

        return new ContentInsightsSummaryDto(
            totalViews,
            deduplicatedViews,
            helpfulCount,
            notHelpfulCount,
            helpfulPercentage,
            totalRatings,
            averageRating,
            globalEngagementRate,
            ratingDistribution,
            negativeFeedbackBreakdown,
            trendPoints,
            topViewed,
            topHelpful,
            highestRated,
            lowestRated,
            mostEngaged,
            needsAttention);
    }

    public async Task<InfographicPagedResult<InfographicInsightDto>> GetGuidesAsync(ContentInsightsGuideQuery query, CancellationToken token = default)
    {
        var fromDate = ResolveFilterDate(query.DateRange);

        var infographicsQuery = db.Infographics.AsNoTracking()
            .Include(x => x.Category)
            .AsQueryable();

        if (query.CategoryId.HasValue)
            infographicsQuery = infographicsQuery.Where(x => x.CategoryId == query.CategoryId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            infographicsQuery = infographicsQuery.Where(x => x.Title.ToLower().Contains(search) || x.Slug.Contains(search));
        }

        var totalItems = await infographicsQuery.CountAsync(token);
        var guides = await infographicsQuery.ToListAsync(token);
        var guideIds = guides.Select(x => x.Id).ToHashSet();

        // Batch query views
        var viewsGrouped = await db.InfographicViews.AsNoTracking()
            .Where(x => guideIds.Contains(x.InfographicId) && x.CreatedAt >= fromDate)
            .GroupBy(x => x.InfographicId)
            .Select(g => new
            {
                InfographicId = g.Key,
                TotalViews = g.Count(),
                DeduplicatedViews = g.Select(v => v.VisitorKeyHash).Distinct().Count(),
                VisitorHashes = g.Select(v => v.VisitorKeyHash).Distinct().ToList()
            })
            .ToListAsync(token);

        // Batch query helpful votes
        var rawVotes = await db.UserHelpfulVotes.AsNoTracking()
            .Where(x => x.EntityType == EntityType && guideIds.Contains(x.EntityId) && x.CreatedAt >= fromDate)
            .Select(x => new { x.EntityId, x.IsHelpful, x.NegativeFeedbackReason, x.VisitorKeyHash })
            .ToListAsync(token);

        var votesGrouped = rawVotes
            .GroupBy(x => x.EntityId)
            .Select(g => new
            {
                InfographicId = g.Key,
                Helpful = g.Count(v => v.IsHelpful),
                NotHelpful = g.Count(v => !v.IsHelpful),
                Voters = g.Select(v => v.VisitorKeyHash).Where(h => h != null).Distinct().Select(h => h!).ToList(),
                NegativeReasons = g.Where(v => !v.IsHelpful && v.NegativeFeedbackReason.HasValue)
                    .GroupBy(v => v.NegativeFeedbackReason!.Value)
                    .Select(rg => new NegativeFeedbackCountDto(rg.Key, rg.Count()))
                    .ToList()
            })
            .ToList();

        // Batch query ratings
        var rawRatings = await db.UserRatings.AsNoTracking()
            .Where(x => x.EntityType == EntityType && guideIds.Contains(x.EntityId) && x.CreatedAt >= fromDate)
            .Select(x => new { x.EntityId, x.Rating, x.VisitorKeyHash })
            .ToListAsync(token);

        var ratingsGrouped = rawRatings
            .GroupBy(x => x.EntityId)
            .Select(g => new
            {
                InfographicId = g.Key,
                Count = g.Count(),
                Average = (decimal)g.Average(r => (double)r.Rating),
                Raters = g.Select(r => r.VisitorKeyHash).Where(h => h != null).Distinct().Select(h => h!).ToList(),
                Distribution = g.GroupBy(r => r.Rating)
                    .Select(dg => new RatingDistributionDto(dg.Key, dg.Count()))
                    .ToList()
            })
            .ToList();

        var guideInsights = guides.Select(guide =>
        {
            var vData = viewsGrouped.FirstOrDefault(x => x.InfographicId == guide.Id);
            var fData = votesGrouped.FirstOrDefault(x => x.InfographicId == guide.Id);
            var rData = ratingsGrouped.FirstOrDefault(x => x.InfographicId == guide.Id);

            var totalViews = vData?.TotalViews ?? 0;
            var dedupViews = vData?.DeduplicatedViews ?? 0;
            var helpful = fData?.Helpful ?? 0;
            var notHelpful = fData?.NotHelpful ?? 0;
            var totalVotes = helpful + notHelpful;
            decimal? helpfulPct = totalVotes == 0
                ? null
                : Math.Round(helpful * 100m / totalVotes, 1);

            var ratingCount = rData?.Count ?? 0;
            decimal? avgRating = ratingCount == 0
                ? null
                : Math.Round(rData!.Average, 2);

            var distribution = Enumerable.Range(1, 5).Reverse()
                .Select(v => new RatingDistributionDto(
                    (byte)v,
                    rData?.Distribution.FirstOrDefault(d => d.Rating == v)?.Count ?? 0))
                .ToList();

            var reasons = fData?.NegativeReasons.OrderByDescending(r => r.Count).ToList()
                          ?? new List<NegativeFeedbackCountDto>();

            var engagedHashes = (fData?.Voters.Where(h => h != null).Select(h => h!) ?? Enumerable.Empty<string>())
                .Union(rData?.Raters.Where(h => h != null).Select(h => h!) ?? Enumerable.Empty<string>())
                .Distinct()
                .Count();

            var engRate = dedupViews == 0
                ? 0m
                : Math.Min(100m, Math.Round(engagedHashes * 100m / dedupViews, 1));

            var (score, status) = CalculateHealth(helpful, notHelpful, avgRating, ratingCount, dedupViews, engRate);

            return new InfographicInsightDto(
                guide.Id,
                guide.Title,
                guide.Slug,
                guide.Category.Name,
                guide.Status,
                guide.DifficultyLevel,
                guide.PublishedAt,
                totalViews,
                dedupViews,
                helpful,
                notHelpful,
                helpfulPct,
                ratingCount,
                avgRating,
                distribution,
                reasons,
                engRate,
                score,
                status,
                Array.Empty<ContentInsightTrendPointDto>());
        }).ToList();

        // Sort
        var isDesc = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        guideInsights = (query.SortBy?.ToLowerInvariant()) switch
        {
            "views" => isDesc ? guideInsights.OrderByDescending(x => x.TotalViews).ToList() : guideInsights.OrderBy(x => x.TotalViews).ToList(),
            "helpful" => isDesc ? guideInsights.OrderByDescending(x => x.HelpfulPercentage ?? -1).ToList() : guideInsights.OrderBy(x => x.HelpfulPercentage ?? -1).ToList(),
            "rating" => isDesc ? guideInsights.OrderByDescending(x => x.AverageRating ?? -1).ToList() : guideInsights.OrderBy(x => x.AverageRating ?? -1).ToList(),
            "engagement" => isDesc ? guideInsights.OrderByDescending(x => x.EngagementRate).ToList() : guideInsights.OrderBy(x => x.EngagementRate).ToList(),
            "health" => isDesc ? guideInsights.OrderByDescending(x => x.HealthScore ?? -1).ToList() : guideInsights.OrderBy(x => x.HealthScore ?? -1).ToList(),
            _ => isDesc ? guideInsights.OrderByDescending(x => x.TotalViews).ToList() : guideInsights.OrderBy(x => x.TotalViews).ToList()
        };

        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);
        var pagedItems = guideInsights.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new InfographicPagedResult<InfographicInsightDto>(pagedItems, page, pageSize, totalItems);
    }

    public async Task<InfographicInsightDto?> GetGuideDetailsAsync(Guid id, ContentInsightsFilter filter, CancellationToken token = default)
    {
        var guide = await db.Infographics.AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, token);

        if (guide is null) return null;

        var fromDate = ResolveFilterDate(filter.DateRange);

        // Views
        var viewsQuery = db.InfographicViews.AsNoTracking()
            .Where(x => x.InfographicId == id && x.CreatedAt >= fromDate);
        var totalViews = await viewsQuery.CountAsync(token);
        var dedupViews = await viewsQuery
            .Select(x => x.VisitorKeyHash)
            .Distinct()
            .CountAsync(token);

        // Helpful votes
        var votesQuery = db.UserHelpfulVotes.AsNoTracking()
            .Where(x => x.EntityType == EntityType && x.EntityId == id && x.CreatedAt >= fromDate);
        var helpful = await votesQuery.CountAsync(x => x.IsHelpful, token);
        var notHelpful = await votesQuery.CountAsync(x => !x.IsHelpful, token);
        var totalVotes = helpful + notHelpful;
        decimal? helpfulPct = totalVotes == 0
            ? null
            : Math.Round(helpful * 100m / totalVotes, 1);

        var negativeReasonsList = await votesQuery
            .Where(x => !x.IsHelpful && x.NegativeFeedbackReason != null)
            .Select(x => x.NegativeFeedbackReason!.Value)
            .ToListAsync(token);

        var negativeReasons = negativeReasonsList
            .GroupBy(x => x)
            .Select(g => new NegativeFeedbackCountDto(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        // Ratings
        var ratingsQuery = db.UserRatings.AsNoTracking()
            .Where(x => x.EntityType == EntityType && x.EntityId == id && x.CreatedAt >= fromDate);
        var guideRatings = await ratingsQuery.Select(r => r.Rating).ToListAsync(token);
        var ratingCount = guideRatings.Count;
        var avgRating = ratingCount == 0
            ? (decimal?)null
            : Math.Round((decimal)guideRatings.Average(r => (double)r), 2);

        var distributionCounts = guideRatings
            .GroupBy(x => x)
            .ToDictionary(g => g.Key, g => g.Count());

        var distribution = Enumerable.Range(1, 5).Reverse()
            .Select(v => new RatingDistributionDto(
                (byte)v,
                distributionCounts.GetValueOrDefault((byte)v, 0)))
            .ToList();

        // Engagement rate
        var voterHashes = await votesQuery.Where(v => v.VisitorKeyHash != null).Select(v => v.VisitorKeyHash!).Distinct().ToListAsync(token);
        var raterHashes = await ratingsQuery.Where(r => r.VisitorKeyHash != null).Select(r => r.VisitorKeyHash!).Distinct().ToListAsync(token);
        var engagedCount = voterHashes.Union(raterHashes).Distinct().Count();
        var engRate = dedupViews == 0
            ? 0m
            : Math.Min(100m, Math.Round(engagedCount * 100m / dedupViews, 1));

        var (score, status) = CalculateHealth(helpful, notHelpful, avgRating, ratingCount, dedupViews, engRate);

        var trend = await BuildTrendAsync(new HashSet<Guid> { id }, fromDate, token);

        return new InfographicInsightDto(
            guide.Id,
            guide.Title,
            guide.Slug,
            guide.Category.Name,
            guide.Status,
            guide.DifficultyLevel,
            guide.PublishedAt,
            totalViews,
            dedupViews,
            helpful,
            notHelpful,
            helpfulPct,
            ratingCount,
            avgRating,
            distribution,
            negativeReasons,
            engRate,
            score,
            status,
            trend);
    }

    private async Task<IReadOnlyList<ContentInsightTrendPointDto>> BuildTrendAsync(
        HashSet<Guid> guideIds, DateTime fromDate, CancellationToken token)
    {
        var daysCount = Math.Max(7, (int)Math.Ceiling((DateTime.UtcNow.Date - fromDate.Date).TotalDays) + 1);
        if (daysCount > 90) daysCount = 90;

        var dates = Enumerable.Range(0, daysCount)
            .Select(offset => DateTime.UtcNow.Date.AddDays(-daysCount + 1 + offset))
            .ToList();

        var earliest = dates.First();

        var viewDates = await db.InfographicViews.AsNoTracking()
            .Where(x => guideIds.Contains(x.InfographicId) && x.CreatedAt >= earliest)
            .Select(x => x.CreatedAt)
            .ToListAsync(token);

        var helpfulDates = await db.UserHelpfulVotes.AsNoTracking()
            .Where(x => x.EntityType == EntityType && guideIds.Contains(x.EntityId) && x.CreatedAt >= earliest && x.IsHelpful)
            .Select(x => x.CreatedAt)
            .ToListAsync(token);

        var notHelpfulDates = await db.UserHelpfulVotes.AsNoTracking()
            .Where(x => x.EntityType == EntityType && guideIds.Contains(x.EntityId) && x.CreatedAt >= earliest && !x.IsHelpful)
            .Select(x => x.CreatedAt)
            .ToListAsync(token);

        var ratingDates = await db.UserRatings.AsNoTracking()
            .Where(x => x.EntityType == EntityType && guideIds.Contains(x.EntityId) && x.CreatedAt >= earliest)
            .Select(x => x.CreatedAt)
            .ToListAsync(token);

        var viewMap = viewDates.GroupBy(x => DateOnly.FromDateTime(x.Date)).ToDictionary(g => g.Key, g => g.Count());
        var helpfulMap = helpfulDates.GroupBy(x => DateOnly.FromDateTime(x.Date)).ToDictionary(g => g.Key, g => g.Count());
        var notHelpfulMap = notHelpfulDates.GroupBy(x => DateOnly.FromDateTime(x.Date)).ToDictionary(g => g.Key, g => g.Count());
        var ratingMap = ratingDates.GroupBy(x => DateOnly.FromDateTime(x.Date)).ToDictionary(g => g.Key, g => g.Count());

        return dates.Select(d =>
        {
            var dateOnly = DateOnly.FromDateTime(d);
            return new ContentInsightTrendPointDto(
                d.ToString("yyyy-MM-dd"),
                viewMap.GetValueOrDefault(dateOnly, 0),
                helpfulMap.GetValueOrDefault(dateOnly, 0),
                notHelpfulMap.GetValueOrDefault(dateOnly, 0),
                ratingMap.GetValueOrDefault(dateOnly, 0));
        }).ToList();
    }

    private static (int? Score, string Status) CalculateHealth(
        int helpful, int notHelpful, decimal? avgRating, int ratingCount, int dedupViews, decimal engRate)
    {
        var totalFeedback = helpful + notHelpful;
        if ((totalFeedback + ratingCount) < 3 || dedupViews < 5)
            return (null, "Insufficient data");

        var helpfulComponent = totalFeedback > 0
            ? (helpful * 100m / totalFeedback)
            : 70m;

        var ratingComponent = avgRating.HasValue
            ? (avgRating.Value * 20m)
            : 70m;

        var engagementComponent = Math.Min(engRate * 2.0m, 100m);

        var composite = (helpfulComponent * 0.40m) + (ratingComponent * 0.40m) + (engagementComponent * 0.20m);

        if (totalFeedback >= 3 && notHelpful > helpful)
            composite -= 10m;

        var score = Math.Clamp((int)Math.Round(composite), 0, 100);

        var status = score switch
        {
            >= 85 => "Excellent",
            >= 70 => "Good",
            >= 50 => "Needs Attention",
            _ => "Critical"
        };

        return (score, status);
    }

    private static DateTime ResolveFilterDate(string? dateRange) => (dateRange?.ToLowerInvariant()) switch
    {
        "7d" => DateTime.UtcNow.AddDays(-7),
        "30d" => DateTime.UtcNow.AddDays(-30),
        "90d" => DateTime.UtcNow.AddDays(-90),
        "all" => DateTime.MinValue,
        _ => DateTime.UtcNow.AddDays(-30)
    };

    private static string GetReasonLabel(NegativeFeedbackReason reason) => reason switch
    {
        NegativeFeedbackReason.NeedsRealWorldExample => "Needs a real-world example",
        NegativeFeedbackReason.ExplanationUnclear => "Explanation was unclear",
        NegativeFeedbackReason.TooBasic => "Too basic",
        NegativeFeedbackReason.TooAdvanced => "Too advanced",
        NegativeFeedbackReason.NeedsMoreDetail => "Needs more detail",
        NegativeFeedbackReason.MayBeOutdated => "May be outdated",
        NegativeFeedbackReason.Other => "Other",
        _ => reason.ToString()
    };
}
