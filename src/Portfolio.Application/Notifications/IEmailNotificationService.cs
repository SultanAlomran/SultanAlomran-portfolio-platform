using Portfolio.Application.Contact;

namespace Portfolio.Application.Notifications;

public interface IEmailNotificationService
{
    Task SendContactMessageNotificationAsync(
        ContactNotificationEvent notification,
        CancellationToken cancellationToken = default);
}
