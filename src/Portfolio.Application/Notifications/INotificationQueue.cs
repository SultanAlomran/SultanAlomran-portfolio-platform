using Portfolio.Application.Contact;

namespace Portfolio.Application.Notifications;

public interface INotificationQueue
{
    ValueTask EnqueueAsync(
        ContactNotificationEvent notification,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<ContactNotificationEvent> ReadAllAsync(
        CancellationToken cancellationToken);
}
