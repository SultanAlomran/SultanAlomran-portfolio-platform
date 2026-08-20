namespace Portfolio.Application.Contact;

public interface IContactService
{
    Task<PublicContactSubmissionResponse> SubmitContactMessageAsync(
        CreateContactMessageRequest request,
        string? pageRoute = null,
        string? referrer = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    Task<ContactPagedResult<ContactMessageSummaryDto>> GetAdminMessagesAsync(
        ContactMessageQuery query,
        CancellationToken cancellationToken = default);

    Task<ContactMessageDto?> GetAdminMessageByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ContactMessageDto> MarkAsReadAsync(
        Guid id,
        Guid? adminUserId,
        CancellationToken cancellationToken = default);

    Task<ContactMessageDto> MarkAsUnreadAsync(
        Guid id,
        Guid? adminUserId,
        CancellationToken cancellationToken = default);

    Task<ContactMessageDto> ArchiveAsync(
        Guid id,
        Guid? adminUserId,
        CancellationToken cancellationToken = default);

    Task<ContactUnreadCountDto> GetUnreadCountAsync(
        CancellationToken cancellationToken = default);
}
