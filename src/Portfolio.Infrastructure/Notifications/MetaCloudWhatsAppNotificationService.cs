using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Contact;
using Portfolio.Application.Notifications;

namespace Portfolio.Infrastructure.Notifications;

public sealed class MetaCloudWhatsAppNotificationService(
    HttpClient httpClient,
    IOptions<NotificationOptions> options,
    ILogger<MetaCloudWhatsAppNotificationService> logger) : IWhatsAppNotificationService
{
    public async Task SendContactMessageNotificationAsync(ContactNotificationEvent notification, CancellationToken cancellationToken = default)
    {
        var waOptions = options.Value.WhatsApp;
        if (!waOptions.Enabled)
        {
            logger.LogInformation("WhatsApp notifications disabled. Skipping message {MessageId}", notification.Id);
            return;
        }

        var accessToken = waOptions.AccessToken?.Trim().Trim('<', '>', '"', '\'');
        var phoneNumberId = waOptions.PhoneNumberId?.Trim().Trim('<', '>', '"', '\'');

        if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(phoneNumberId))
        {
            logger.LogWarning(
                "Meta WhatsApp Business Cloud API AccessToken or PhoneNumberId is not configured. Skipping WhatsApp notification for message {MessageId}.",
                notification.Id);
            return;
        }

        try
        {
            var adminLink = $"{options.Value.AdminBaseUrl.TrimEnd('/')}/messages";
            var recipient = (waOptions.RecipientPhoneNumber ?? "+966508334411")
                .Trim()
                .Trim('<', '>', '"', '\'')
                .Replace("+", "")
                .Replace(" ", "")
                .Replace("-", "");

            var textBody = $"""
                *New Portfolio Contact*

                *From:* {notification.SenderName}
                *Email:* {notification.SenderEmail}
                *Subject:* {notification.Subject}
                *Received:* {notification.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC

                *Open Admin:*
                {adminLink}
                """;

            var payload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = recipient,
                type = "text",
                text = new
                {
                    preview_url = false,
                    body = textBody
                }
            };

            var apiUrl = string.IsNullOrWhiteSpace(waOptions.ApiUrl)
                ? "https://graph.facebook.com/v21.0"
                : waOptions.ApiUrl.Trim().Trim('<', '>', '"', '\'').TrimEnd('/');

            var url = $"{apiUrl}/{phoneNumberId}/messages";
            using var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogInformation(
                    "Meta Cloud WhatsApp notification sent for message {MessageId}. Response: {Response}",
                    notification.Id,
                    responseContent);
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                logger.LogWarning(
                    "Meta Cloud WhatsApp API returned HTTP {StatusCode} for message {MessageId}. Error: {Error}",
                    (int)response.StatusCode,
                    notification.Id,
                    errorBody);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send Meta Cloud WhatsApp notification for message {MessageId}",
                notification.Id);
        }
    }
}
