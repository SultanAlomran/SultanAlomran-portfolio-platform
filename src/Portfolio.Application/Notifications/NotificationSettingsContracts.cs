namespace Portfolio.Application.Notifications;

public sealed record NotificationSettingsDto(
    bool EmailEnabled,
    bool WhatsAppEnabled,
    bool AdminToastEnabled,
    string EmailProvider,
    string WhatsAppProvider,
    string RecipientEmail,
    string RecipientPhoneNumber);

public sealed record UpdateNotificationSettingsRequest(
    bool EmailEnabled,
    bool WhatsAppEnabled,
    bool AdminToastEnabled);

public interface INotificationSettingsService
{
    Task<NotificationSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<NotificationSettingsDto> UpdateSettingsAsync(UpdateNotificationSettingsRequest request, CancellationToken cancellationToken = default);
    Task<bool> IsEmailEnabledAsync(CancellationToken cancellationToken = default);
    Task<bool> IsWhatsAppEnabledAsync(CancellationToken cancellationToken = default);
    Task<bool> IsAdminToastEnabledAsync(CancellationToken cancellationToken = default);
}
