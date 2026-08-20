using Portfolio.Application.Contact;

namespace Portfolio.Application.Notifications;

public interface IWhatsAppNotificationService
{
    Task SendContactMessageNotificationAsync(
        ContactNotificationEvent notification,
        CancellationToken cancellationToken = default);
}
