using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class UserHelpfulVote : Entity
{
    private UserHelpfulVote() { }
    public Guid? UserId { get; private set; }
    public string? VisitorKeyHash { get; private set; }
    public string EntityType { get; private set; } = "";
    public Guid EntityId { get; private set; }
    public bool IsHelpful { get; private set; }
    public NegativeFeedbackReason? NegativeFeedbackReason { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }
    public User? User { get; private set; }

    public static UserHelpfulVote ForVisitor(string visitorKeyHash, string entityType, Guid entityId,
        bool isHelpful, NegativeFeedbackReason? reason = null)
    {
        if (string.IsNullOrWhiteSpace(visitorKeyHash))
            throw new ArgumentException("Visitor key hash is required.", nameof(visitorKeyHash));
        var vote = new UserHelpfulVote
        {
            VisitorKeyHash = visitorKeyHash,
            EntityType = entityType,
            EntityId = entityId
        };
        vote.SetVote(isHelpful, reason);
        vote.UpdatedAt = null;
        return vote;
    }

    public void SetVote(bool isHelpful, NegativeFeedbackReason? reason = null)
    {
        if (isHelpful && reason.HasValue)
            throw new ArgumentException("Helpful votes cannot include a negative feedback reason.", nameof(reason));
        if (reason.HasValue && !Enum.IsDefined(reason.Value))
            throw new ArgumentOutOfRangeException(nameof(reason));
        IsHelpful = isHelpful;
        NegativeFeedbackReason = isHelpful ? null : reason;
        UpdatedAt = DateTime.UtcNow;
    }

}
