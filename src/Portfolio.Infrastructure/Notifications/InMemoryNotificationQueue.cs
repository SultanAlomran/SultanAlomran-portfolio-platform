using System.Threading.Channels;
using Portfolio.Application.Contact;
using Portfolio.Application.Notifications;

namespace Portfolio.Infrastructure.Notifications;

public sealed class InMemoryNotificationQueue : INotificationQueue
{
    private readonly Channel<ContactNotificationEvent> _channel = Channel.CreateUnbounded<ContactNotificationEvent>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask EnqueueAsync(ContactNotificationEvent notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        return _channel.Writer.WriteAsync(notification, cancellationToken);
    }

    public IAsyncEnumerable<ContactNotificationEvent> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
