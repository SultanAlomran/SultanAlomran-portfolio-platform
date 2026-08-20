using Portfolio.Application.Contact;

namespace Portfolio.Application.Notifications;

public interface IAdminRealtimeNotifier
{
    Task NotifyNewContactMessageAsync(
        ContactNotificationEvent notification,
        CancellationToken cancellationToken = default);

    Task NotifyUnreadCountChangedAsync(
        int unreadCount,
        CancellationToken cancellationToken = default);
}
