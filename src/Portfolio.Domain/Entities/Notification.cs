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
    public void MarkAsRead() { IsRead = true; ReadAt = DateTime.UtcNow; }
}
