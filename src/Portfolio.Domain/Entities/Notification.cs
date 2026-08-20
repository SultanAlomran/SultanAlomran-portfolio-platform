using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Notification : Entity
{
    private Notification() { }
    public Guid? UserId { get; private set; }
    public string Title { get; private set; } = "";
    public string Message { get; private set; } = "";
    public NotificationType Type { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public User? User { get; private set; }
    public static Notification Create(string title, string message, NotificationType type = NotificationType.Info, Guid? userId = null)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentException("Message is required.", nameof(message));

        return new Notification
        {
            Title = title.Trim(),
            Message = message.Trim(),
            Type = type,
            UserId = userId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
    }
    public void MarkAsRead() { IsRead = true; ReadAt = DateTime.UtcNow; }
}
