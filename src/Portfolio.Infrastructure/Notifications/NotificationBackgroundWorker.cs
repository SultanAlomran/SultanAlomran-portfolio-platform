using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Portfolio.Application.Contact;
using Portfolio.Application.Notifications;

namespace Portfolio.Infrastructure.Notifications;

public sealed class NotificationBackgroundWorker(
    INotificationQueue queue,
    IEmailNotificationService emailService,
    IWhatsAppNotificationService whatsAppService,
    ILogger<NotificationBackgroundWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Notification background worker started.");

        try
        {
            await foreach (var notification in queue.ReadAllAsync(stoppingToken))
            {
                await ProcessNotificationAsync(notification, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Notification background worker is stopping due to cancellation.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in notification background worker loop.");
        }
    }

    private async Task ProcessNotificationAsync(ContactNotificationEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing background notification for message {MessageId} from {SenderEmail}.",
            notification.Id,
            notification.SenderEmail);

        // Independent Email dispatch
        try
        {
            await DispatchWithRetryAsync(
                "Email",
                () => emailService.SendContactMessageNotificationAsync(notification, cancellationToken),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deliver Email notification for message {MessageId}.", notification.Id);
        }

        // Independent WhatsApp dispatch
        try
        {
            await DispatchWithRetryAsync(
                "WhatsApp",
                () => whatsAppService.SendContactMessageNotificationAsync(notification, cancellationToken),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to deliver WhatsApp notification for message {MessageId}.", notification.Id);
        }
    }

    private static async Task DispatchWithRetryAsync(string channelName, Func<Task> action, CancellationToken cancellationToken)
    {
        const int maxRetries = 2;
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await action();
                return;
            }
            catch when (attempt < maxRetries && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
            }
        }
    }
}
