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

    
}
