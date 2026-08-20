using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Portfolio.Application.Contact;
using Portfolio.Application.Notifications;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Contact;

public sealed class ContactService(
    PortfolioDbContext db,
    INotificationQueue notificationQueue,
    IAdminRealtimeNotifier realtimeNotifier,
    ILogger<ContactService> logger) : IContactService
{
    public async Task<PublicContactSubmissionResponse> SubmitContactMessageAsync(
        CreateContactMessageRequest request,
        string? pageRoute = null,
        string? referrer = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        ValidateSubmission(request);

        var message = ContactMessage.Create(
            request.Name,
            request.Email,
            request.Subject,
            request.Message,
            pageRoute,
            referrer);

        db.ContactMessages.Add(message);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Contact message {MessageId} persisted successfully from {SenderEmail}.",
            message.Id,
            message.Email);

        var unreadCount = await db.ContactMessages.CountAsync(x => x.Status == ContactStatus.New, cancellationToken);
        var preview = message.Message.Length > 160 ? $"{message.Message[..157]}..." : message.Message;

        var notificationEvent = new ContactNotificationEvent(
            message.Id,
            message.Name,
            message.Email,
            message.Subject,
            preview,
            message.CreatedAt,
            unreadCount);

        // Realtime notification to connected Admin clients (isolated from request outcome)
        try
        {
            await realtimeNotifier.NotifyNewContactMessageAsync(notificationEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to publish realtime SignalR notification for message {MessageId}.", message.Id);
        }

        // Asynchronous background dispatch for Email and WhatsApp
        try
        {
            await notificationQueue.EnqueueAsync(notificationEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to enqueue background notification for message {MessageId}.", message.Id);
        }

        return new PublicContactSubmissionResponse(
            message.Id,
            "Thank you! Your message has been sent successfully. Sultan has been notified via Email and WhatsApp.",
            message.CreatedAt);
    }

    public async Task<ContactPagedResult<ContactMessageSummaryDto>> GetAdminMessagesAsync(
        ContactMessageQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        IQueryable<ContactMessage> messages = db.ContactMessages.AsNoTracking();

        if (query.Status.HasValue)
        {
            messages = messages.Where(x => x.Status == query.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            messages = messages.Where(x =>
                x.Name.Contains(search) ||
                x.Email.Contains(search) ||
                x.Subject.Contains(search) ||
                x.Message.Contains(search));
        }

        var totalCount = await messages.CountAsync(cancellationToken);

        var items = await messages
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ContactMessageSummaryDto(
                x.Id,
                x.Name,
                x.Email,
                x.Subject,
                x.Message.Length > 160 ? x.Message.Substring(0, 157) + "..." : x.Message,
                x.Status,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return new ContactPagedResult<ContactMessageSummaryDto>(items, totalCount, page, pageSize);
    }

    public async Task<ContactMessageDto?> GetAdminMessageByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var message = await db.ContactMessages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return message is null ? null : MapToDto(message);
    }

    public async Task<ContactMessageDto> MarkAsReadAsync(
        Guid id,
        Guid? adminUserId,
        CancellationToken cancellationToken = default)
    {
        var message = await db.ContactMessages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Contact message with ID '{id}' was not found.");

        if (message.Status == ContactStatus.New)
        {
            message.MarkAsRead();
            db.AuditLogs.Add(AuditLog.Create("Contact.MessageRead", "ContactMessage", adminUserId, message.Id));
            await db.SaveChangesAsync(cancellationToken);

            await NotifyUnreadCountAsync(cancellationToken);
        }

        return MapToDto(message);
    }

    public async Task<ContactMessageDto> MarkAsUnreadAsync(
        Guid id,
        Guid? adminUserId,
        CancellationToken cancellationToken = default)
    {
        var message = await db.ContactMessages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Contact message with ID '{id}' was not found.");

        if (message.Status != ContactStatus.New)
        {
            message.MarkAsUnread();
            db.AuditLogs.Add(AuditLog.Create("Contact.MessageUnread", "ContactMessage", adminUserId, message.Id));
            await db.SaveChangesAsync(cancellationToken);

            await NotifyUnreadCountAsync(cancellationToken);
        }

        return MapToDto(message);
    }

    public async Task<ContactMessageDto> ArchiveAsync(
        Guid id,
        Guid? adminUserId,
        CancellationToken cancellationToken = default)
    {
        var message = await db.ContactMessages.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Contact message with ID '{id}' was not found.");

        if (message.Status != ContactStatus.Archived)
        {
            message.Archive();
            db.AuditLogs.Add(AuditLog.Create("Contact.MessageArchived", "ContactMessage", adminUserId, message.Id));
            await db.SaveChangesAsync(cancellationToken);

            await NotifyUnreadCountAsync(cancellationToken);
        }

        return MapToDto(message);
    }

    public async Task<ContactUnreadCountDto> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        var unread = await db.ContactMessages.CountAsync(x => x.Status == ContactStatus.New, cancellationToken);
        var total = await db.ContactMessages.CountAsync(cancellationToken);
        return new ContactUnreadCountDto(unread, total);
    }

    private async Task NotifyUnreadCountAsync(CancellationToken cancellationToken)
    {
        try
        {
            var unreadCount = await db.ContactMessages.CountAsync(x => x.Status == ContactStatus.New, cancellationToken);
            await realtimeNotifier.NotifyUnreadCountChangedAsync(unreadCount, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to broadcast unread count update via SignalR.");
        }
    }

    private static void ValidateSubmission(CreateContactMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 150)
            throw new ArgumentException("Full Name is required and must not exceed 150 characters.", nameof(request.Name));

        if (string.IsNullOrWhiteSpace(request.Email) || request.Email.Trim().Length > 320 || !IsValidEmail(request.Email.Trim()))
            throw new ArgumentException("A valid email address is required (maximum 320 characters).", nameof(request.Email));

        if (string.IsNullOrWhiteSpace(request.Subject) || request.Subject.Trim().Length > 250)
            throw new ArgumentException("Subject is required and must not exceed 250 characters.", nameof(request.Subject));

        if (string.IsNullOrWhiteSpace(request.Message) || request.Message.Trim().Length > 4000)
            throw new ArgumentException("Message is required and must not exceed 4000 characters.", nameof(request.Message));
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static ContactMessageDto MapToDto(ContactMessage message) => new(
        message.Id,
        message.Name,
        message.Email,
        message.Subject,
        message.Message,
        message.Status,
        message.CreatedAt,
        message.UpdatedAt,
        message.PageRoute,
        message.Referrer);
}
