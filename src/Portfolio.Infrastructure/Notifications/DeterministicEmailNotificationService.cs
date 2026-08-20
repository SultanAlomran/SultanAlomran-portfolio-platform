using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Contact;
using Portfolio.Application.Notifications;

namespace Portfolio.Infrastructure.Notifications;

public sealed class DeterministicEmailNotificationService(
    IOptions<NotificationOptions> options,
    ILogger<DeterministicEmailNotificationService> logger) : IEmailNotificationService
{
    public Task SendContactMessageNotificationAsync(
        ContactNotificationEvent notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var config = options.Value.Email;
        if (!config.Enabled)
        {
            logger.LogInformation(
                "Email notifications are disabled. Skipped notification for message {MessageId} from {SenderEmail}.",
                notification.Id,
                notification.SenderEmail);
            return Task.CompletedTask;
        }

        var adminLink = $"{options.Value.AdminBaseUrl.TrimEnd('/')}/messages";

        logger.LogInformation(
            "Dispatched Email notification for message {MessageId} [From: {SenderName} <{SenderEmail}>, Subject: {Subject}, To: {RecipientEmail}, Link: {AdminLink}]",
            notification.Id,
            notification.SenderName,
            notification.SenderEmail,
            notification.Subject,
            config.RecipientEmail,
            adminLink);

        return Task.CompletedTask;
    }
}
