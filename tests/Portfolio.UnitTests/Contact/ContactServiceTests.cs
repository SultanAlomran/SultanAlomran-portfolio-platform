using Portfolio.Application.Contact;
using Portfolio.Domain.Entities;
using Portfolio.Domain.Enums;

namespace Portfolio.UnitTests.Contact;

public sealed class ContactMessageUnitTests
{
    [Fact]
    public void CreateContactMessageRequest_holds_valid_data()
    {
        var request = new CreateContactMessageRequest(
            "Ahmed Alomran",
            "ahmed@example.com",
            "Senior .NET Opportunity",
            "Hello Sultan, we would like to discuss an opportunity.");

        Assert.Equal("Ahmed Alomran", request.Name);
        Assert.Equal("ahmed@example.com", request.Email);
        Assert.Equal("Senior .NET Opportunity", request.Subject);
        Assert.Equal("Hello Sultan, we would like to discuss an opportunity.", request.Message);
    }

    [Fact]
    public void ContactNotificationEvent_contains_minimal_summary_fields()
    {
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var evt = new ContactNotificationEvent(
            id,
            "Ahmed Alomran",
            "ahmed@example.com",
            "Senior .NET Opportunity",
            "Hello Sultan...",
            now,
            3);

        Assert.Equal(id, evt.Id);
        Assert.Equal("Ahmed Alomran", evt.SenderName);
        Assert.Equal("ahmed@example.com", evt.SenderEmail);
        Assert.Equal("Senior .NET Opportunity", evt.Subject);
        Assert.Equal("Hello Sultan...", evt.Preview);
        Assert.Equal(now, evt.CreatedAt);
        Assert.Equal(3, evt.UnreadCount);
    }

    [Fact]
    public void ContactMessage_domain_entity_enforces_lifecycle_and_invariants()
    {
        var message = ContactMessage.Create(
            "Ahmed Alomran",
            "Ahmed@Example.Com ",
            "Project Discussion",
            "Hello, let's connect.");

        Assert.Equal("Ahmed Alomran", message.Name);
        Assert.Equal("ahmed@example.com", message.Email);
        Assert.Equal(ContactStatus.New, message.Status);

        message.MarkAsRead();
        Assert.Equal(ContactStatus.Read, message.Status);

        message.MarkAsUnread();
        Assert.Equal(ContactStatus.New, message.Status);

        message.Archive();
        Assert.Equal(ContactStatus.Archived, message.Status);
    }
}
