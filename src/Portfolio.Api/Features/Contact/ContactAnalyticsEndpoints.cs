using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Authentication;
using Portfolio.Application.Contact;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Api.Features.Contact;

public static class ContactAnalyticsEndpoints
{
    public static RouteGroupBuilder MapContactAnalyticsEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/analytics", GetAnalyticsAsync)
            .WithName("GetContactAnalytics")
            .WithSummary("Retrieve aggregated metrics and historical trend data for contact messages.")
            .Produces<ContactAnalyticsDto>(StatusCodes.Status200OK)
            .RequireAuthorization(AdminAuthorization.Policy);

        return group;
    }

    private static async Task<IResult> GetAnalyticsAsync(
        [FromServices] PortfolioDbContext db,
        CancellationToken cancellationToken)
    {
        var totalMessages = await db.ContactMessages.CountAsync(cancellationToken);
        var newMessages = await db.ContactMessages.CountAsync(x => x.Status == ContactStatus.New, cancellationToken);
        var readMessagesCount = await db.ContactMessages.CountAsync(x => x.Status == ContactStatus.Read, cancellationToken);
        var archivedMessages = await db.ContactMessages.CountAsync(x => x.Status == ContactStatus.Archived, cancellationToken);

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var messagesThisMonth = await db.ContactMessages.CountAsync(x => x.CreatedAt >= startOfMonth, cancellationToken);

        var readMessages = await db.ContactMessages
            .Where(x => x.Status == ContactStatus.Read && x.UpdatedAt.HasValue)
            .Select(x => new { x.CreatedAt, ReadAt = x.UpdatedAt!.Value })
            .ToListAsync(cancellationToken);

        double? avgResponseHours = readMessages.Count > 0
            ? Math.Round(readMessages.Average(x => (x.ReadAt - x.CreatedAt).TotalHours), 1)
            : null;

        var thirtyDaysAgo = DateTime.UtcNow.Date.AddDays(-29);
        var recentDates = await db.ContactMessages
            .Where(x => x.CreatedAt >= thirtyDaysAgo)
            .Select(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var trendMap = recentDates
            .GroupBy(x => DateOnly.FromDateTime(x.Date))
            .ToDictionary(g => g.Key, g => g.Count());

        var trend = Enumerable.Range(0, 30)
            .Select(offset => DateOnly.FromDateTime(thirtyDaysAgo.AddDays(offset)))
            .Select(date => new DailyMessageTrendDto(date, trendMap.GetValueOrDefault(date, 0)))
            .ToList();

        var subjects = await db.ContactMessages
            .Select(x => x.Subject)
            .ToListAsync(cancellationToken);

        var topSubjects = subjects
            .GroupBy(x => x.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => new TopSubjectDto(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();

        var result = new ContactAnalyticsDto(
            TotalMessages: totalMessages,
            NewMessages: newMessages,
            ReadMessages: readMessagesCount,
            ArchivedMessages: archivedMessages,
            MessagesThisMonth: messagesThisMonth,
            AverageResponseTimeHours: avgResponseHours,
            Trend: trend,
            TopSubjects: topSubjects);

        return Results.Ok(result);
    }
}
