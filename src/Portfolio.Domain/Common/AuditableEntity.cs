namespace Portfolio.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
}
