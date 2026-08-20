using Portfolio.Domain.Enums;

namespace Portfolio.Application.Contact;

public sealed record CreateContactMessageRequest(
    string Name,
    string Email,
    string Subject,
    string Message);

public sealed record PublicContactSubmissionResponse(
    Guid Id,
    string Message,
    DateTime ReceivedAtUtc);

public sealed record ContactMessageDto(
    Guid Id,
    string Name,
    string Email,
    string Subject,
    string Message,
    ContactStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? PageRoute,
    string? Referrer);

public sealed record ContactMessageSummaryDto(
    Guid Id,
    string Name,
    string Email,
    string Subject,
    string Preview,
    ContactStatus Status,
    DateTime CreatedAt);

public sealed record ContactMessageQuery(
    string? Search = null,
    ContactStatus? Status = null,
    int Page = 1,
    int PageSize = 20);

public sealed record ContactUnreadCountDto(
    int UnreadCount,
    int TotalCount);

public sealed record ContactNotificationEvent(
    Guid Id,
    string SenderName,
    string SenderEmail,
    string Subject,
    string Preview,
    DateTime CreatedAt,
    int UnreadCount);

public sealed record ContactPagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);
