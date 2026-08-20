using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Contact;
using Portfolio.Application.Notifications;

namespace Portfolio.Infrastructure.Notifications;

public sealed class AzureCommunicationEmailNotificationService(
    IOptions<NotificationOptions> options,
    ILogger<AzureCommunicationEmailNotificationService> logger) : IEmailNotificationService
{
    public async Task SendContactMessageNotificationAsync(ContactNotificationEvent notification, CancellationToken cancellationToken = default)
    {
        var emailOptions = options.Value.Email;
        if (!emailOptions.Enabled)
        {
            logger.LogInformation("Email notifications disabled. Skipping message {MessageId}", notification.Id);
            return;
        }

        var rawConnectionString = emailOptions.ConnectionString?.Trim().Trim('<', '>', '"', '\'');
        if (string.IsNullOrWhiteSpace(rawConnectionString))
        {
            logger.LogWarning(
                "Azure Communication Services Email connection string is not configured. Skipping email delivery for message {MessageId}.",
                notification.Id);
            return;
        }

        try
        {
            var emailClient = new EmailClient(rawConnectionString);
            var subject = $"[Portfolio Contact] {notification.Subject}";
            var adminLink = $"{options.Value.AdminBaseUrl.TrimEnd('/')}/messages";

            var plainTextContent = $"""
                New portfolio contact message received.

                From: {notification.SenderName}
                Email: {notification.SenderEmail}
                Subject: {notification.Subject}
                Received: {notification.CreatedAt:yyyy-MM-dd HH:mm:ss UTC}

                Message Preview:
                {notification.Preview}

                View in Admin Portal:
                {adminLink}
                """;

            var htmlContent = $$"""
                <!DOCTYPE html>
                <html>
                <head>
                  <meta charset="utf-8">
                  <style>
                    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; line-height: 1.6; color: #0f172a; background-color: #f8fafc; margin: 0; padding: 24px; }
                    .card { max-width: 600px; margin: 0 auto; background: #ffffff; border: 1px solid #e2e8f0; border-radius: 16px; padding: 32px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); }
                    .badge { display: inline-block; background: #ede9fe; color: #6d28d9; padding: 4px 12px; border-radius: 9999px; font-size: 12px; font-weight: 700; text-transform: uppercase; }
                    h1 { font-size: 20px; font-weight: 800; margin: 16px 0 8px; color: #0f172a; }
                    .meta { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 12px; padding: 16px; margin: 20px 0; font-size: 14px; }
                    .meta p { margin: 4px 0; }
                    .message-box { background: #ffffff; border-left: 4px solid #7c3aed; padding: 12px 16px; margin: 20px 0; font-size: 14px; color: #334155; white-space: pre-wrap; }
                    .btn { display: inline-block; background: #7c3aed; color: #ffffff !important; padding: 12px 24px; border-radius: 10px; text-decoration: none; font-weight: 700; font-size: 14px; margin-top: 12px; }
                  </style>
                </head>
                <body>
                  <div class="card">
                    <span class="badge">New Contact</span>
                    <h1>{{System.Net.WebUtility.HtmlEncode(notification.Subject)}}</h1>
                    <div class="meta">
                      <p><strong>From:</strong> {{System.Net.WebUtility.HtmlEncode(notification.SenderName)}}</p>
                      <p><strong>Email:</strong> <a href="mailto:{{System.Net.WebUtility.HtmlEncode(notification.SenderEmail)}}">{{System.Net.WebUtility.HtmlEncode(notification.SenderEmail)}}</a></p>
                      <p><strong>Received:</strong> {{notification.CreatedAt:yyyy-MM-dd HH:mm:ss}} UTC</p>
                    </div>
                    <div class="message-box">{{System.Net.WebUtility.HtmlEncode(notification.Preview)}}</div>
                    <a href="{{adminLink}}" class="btn">Open Admin Inbox</a>
                  </div>
                </body>
                </html>
                """;

            var emailMessage = new EmailMessage(
                senderAddress: emailOptions.FromAddress,
                recipientAddress: emailOptions.RecipientEmail,
                content: new EmailContent(subject)
                {
                    PlainText = plainTextContent,
                    Html = htmlContent
                });

            var emailSendOperation = await emailClient.SendAsync(
                WaitUntil.Started,
                emailMessage,
                cancellationToken);

            logger.LogInformation(
                "Azure Communication Services Email queued for message {MessageId}. Operation ID: {OperationId}",
                notification.Id,
                emailSendOperation.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send Azure Communication Services Email notification for message {MessageId}",
                notification.Id);
        }
    }
}
