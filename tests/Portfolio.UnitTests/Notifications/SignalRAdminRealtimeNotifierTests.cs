using Portfolio.Application.Contact;
using Portfolio.Application.Notifications;

namespace Portfolio.UnitTests.Notifications;

public sealed class NotificationUnitTests
{
    [Fact]
    public void NotificationOptions_has_expected_defaults()
    {
        var options = new NotificationOptions();

        Assert.NotNull(options.Email);
        Assert.NotNull(options.WhatsApp);
        Assert.True(options.Email.Enabled);
        Assert.True(options.WhatsApp.Enabled);
        Assert.Equal("Deterministic", options.Email.Provider);
        Assert.Equal("Deterministic", options.WhatsApp.Provider);
        Assert.Equal("+966508334411", options.WhatsApp.RecipientPhoneNumber);
        Assert.Equal("http://localhost:4300", options.AdminBaseUrl);
    }

    [Fact]
    public async Task TestAdminRealtimeNotifierSpy_records_broadcasts_deterministically()
    {
        var spy = new TestAdminRealtimeNotifierSpy();
        var evt = new ContactNotificationEvent(
            Guid.NewGuid(),
            "Ahmed Alomran",
            "ahmed@example.com",
            "Opportunity",
            "Hello Sultan...",
            DateTime.UtcNow,
            2);

        await spy.NotifyNewContactMessageAsync(evt);
        await spy.NotifyUnreadCountChangedAsync(5);

        Assert.Single(spy.NewMessageEvents);
        Assert.Equal(evt.Id, spy.NewMessageEvents[0].Id);
        Assert.Single(spy.UnreadCountEvents);
        Assert.Equal(5, spy.UnreadCountEvents[0]);
    }

    private sealed class TestAdminRealtimeNotifierSpy : IAdminRealtimeNotifier
    {
        public List<ContactNotificationEvent> NewMessageEvents { get; } = [];
        public List<int> UnreadCountEvents { get; } = [];

        public Task NotifyNewContactMessageAsync(ContactNotificationEvent notification, CancellationToken cancellationToken = default)
        {
            NewMessageEvents.Add(notification);
            return Task.CompletedTask;
        }

        public Task NotifyUnreadCountChangedAsync(int unreadCount, CancellationToken cancellationToken = default)
        {
            UnreadCountEvents.Add(unreadCount);
            return Task.CompletedTask;
        }
    }
}
