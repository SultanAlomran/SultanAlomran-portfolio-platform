using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Tag : Entity
{
    private Tag() { }
    public static Tag Create(string name, string slug) => new()
    {
        Name = name.Trim(),
        Slug = slug.Trim().ToLowerInvariant()
    };
    public string Name { get; private set; } = "";
    public string Slug { get; private set; } = "";
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;


}
