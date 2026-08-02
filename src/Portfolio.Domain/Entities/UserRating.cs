using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class UserRating : Entity
{
    private UserRating() { }
    public Guid UserId { get; private set; }
    public string EntityType { get; private set; } = "";
    public Guid EntityId { get; private set; }
    public byte Rating { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public User User { get; private set; } = null!;
    public UserRating(Guid userId, string entityType, Guid entityId, byte rating) { if (rating is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(rating)); UserId = userId; EntityType = entityType; EntityId = entityId; Rating = rating; }
}
