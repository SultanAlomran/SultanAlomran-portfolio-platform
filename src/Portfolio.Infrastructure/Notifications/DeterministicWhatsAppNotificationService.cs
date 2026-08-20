using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Contact;
using Portfolio.Application.Notifications;

namespace Portfolio.Infrastructure.Notifications;

public sealed class DeterministicWhatsAppNotificationService(
    IOptions<NotificationOptions> options,
    ILogger<DeterministicWhatsAppNotificationService> logger) : IWhatsAppNotificationService
{
    public Task SendContactMessageNotificationAsync(
        ContactNotificationEvent notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var config = options.Value.WhatsApp;
        if (!config.Enabled)
        {
            logger.LogInformation(
                "WhatsApp notifications are disabled. Skipped notification for message {MessageId} from {SenderEmail}.",
                notification.Id,
                notification.SenderEmail);
            return Task.CompletedTask;
        }

        var recipient = string.IsNullOrWhiteSpace(config.RecipientPhoneNumber)
            ? "+966508334411"
            : config.RecipientPhoneNumber;

        var adminLink = $"{options.Value.AdminBaseUrl.TrimEnd('/')}/messages";

        logger.LogInformation(
            "Dispatched WhatsApp notification for message {MessageId} [Recipient: {Recipient}, From: {SenderName}, Subject: {Subject}, Link: {AdminLink}]",
            notification.Id,
            recipient,
            notification.SenderName,
            notification.Subject,
            adminLink);

        return Task.CompletedTask;
    }
}
