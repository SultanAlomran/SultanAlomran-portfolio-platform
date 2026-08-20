using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Portfolio.Api.Hubs;
using Portfolio.Application.Contact;
using Portfolio.Application.Notifications;

namespace Portfolio.Api.Features.Notifications;

public sealed class SignalRAdminRealtimeNotifier(
    IHubContext<NotificationsHub> hubContext,
    ILogger<SignalRAdminRealtimeNotifier> logger) : IAdminRealtimeNotifier
{
    public async Task NotifyNewContactMessageAsync(
        ContactNotificationEvent notification,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        logger.LogInformation(
            "Broadcasting real-time SignalR notification for new contact message {MessageId} from {SenderName}.",
            notification.Id,
            notification.SenderName);

        await hubContext.Clients.Group(NotificationsHub.AdminGroupName).SendAsync(
            "ReceiveContactMessageNotification",
            notification,
            cancellationToken);

        await hubContext.Clients.Group(NotificationsHub.AdminGroupName).SendAsync(
            "ReceiveUnreadCount",
            notification.UnreadCount,
            cancellationToken);
    }

    public async Task NotifyUnreadCountChangedAsync(
        int unreadCount,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Broadcasting real-time SignalR unread count update: {UnreadCount}.", unreadCount);

        await hubContext.Clients.Group(NotificationsHub.AdminGroupName).SendAsync(
            "ReceiveUnreadCount",
            unreadCount,
            cancellationToken);
    }
}
