using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class UserRating : Entity
{
    private UserRating() { }
    public Guid? UserId { get; private set; }
    public string? VisitorKeyHash { get; private set; }
    public string EntityType { get; private set; } = "";
    public Guid EntityId { get; private set; }
    public byte Rating { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    public User? User { get; private set; }

    public UserRating(Guid userId, string entityType, Guid entityId, byte rating)
    {
        UserId = userId;
        EntityType = entityType;
        EntityId = entityId;
        SetRating(rating);
        UpdatedAt = null;
    }

    public static UserRating ForVisitor(string visitorKeyHash, string entityType, Guid entityId, byte rating)
    {
        if (string.IsNullOrWhiteSpace(visitorKeyHash))
            throw new ArgumentException("Visitor key hash is required.", nameof(visitorKeyHash));
        var result = new UserRating
        {
            VisitorKeyHash = visitorKeyHash,
            EntityType = entityType,
            EntityId = entityId
        };
        result.SetRating(rating);
        result.UpdatedAt = null;
        return result;
    }

    public void SetRating(byte rating)
    {
        if (rating is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(rating));
        Rating = rating;
        UpdatedAt = DateTime.UtcNow;
    }
}
