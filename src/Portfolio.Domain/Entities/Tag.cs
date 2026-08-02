using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Tag : Entity
{
    private Tag() { }
    public string Name { get; private set; } = "";
    public string Slug { get; private set; } = "";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    
}
