using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class ContactMessage : Entity
{
    private ContactMessage() { }
    public string Name { get; private set; } = "";
    public string Email { get; private set; } = "";
    public string Subject { get; private set; } = "";
    public string Message { get; private set; } = "";
    public string? PageRoute { get; private set; }
    public string? Referrer { get; private set; }
    public ContactStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public static ContactMessage Create(
        string name,
        string email,
        string subject,
        string message,
        string? pageRoute = null,
        string? referrer = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(subject)) throw new ArgumentException("Subject is required.", nameof(subject));
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));

        return new ContactMessage
        {
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Subject = subject.Trim(),
            Message = message.Trim(),
            PageRoute = string.IsNullOrWhiteSpace(pageRoute) ? null : pageRoute.Trim(),
            Referrer = string.IsNullOrWhiteSpace(referrer) ? null : referrer.Trim(),
            Status = ContactStatus.New,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        if (Status == ContactStatus.New)
        {
            Status = ContactStatus.Read;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void MarkAsUnread()
    {
        if (Status != ContactStatus.New)
        {
            Status = ContactStatus.New;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Archive()
    {
        if (Status != ContactStatus.Archived)
        {
            Status = ContactStatus.Archived;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
