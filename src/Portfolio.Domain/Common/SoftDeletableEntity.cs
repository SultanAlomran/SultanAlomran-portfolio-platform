namespace Portfolio.Domain.Common;

public abstract class SoftDeletableEntity : AuditableEntity
{
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public void SoftDelete(Guid? actorId = null) { IsDeleted = true; DeletedAt = DateTime.UtcNow; DeletedBy = actorId; }
    public void Restore() { IsDeleted = false; DeletedAt = null; DeletedBy = null; }
}
