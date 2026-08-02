using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class ExperienceItem : AuditableEntity
{
    private ExperienceItem() { }
    public string JobTitle { get; private set; } = "";
    public string OrganizationName { get; private set; } = "";
    public string? Description { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly? EndDate { get; private set; }
    public bool IsCurrent { get; private set; }
    public int DisplayOrder { get; private set; }

    public void SetDates(DateOnly start, DateOnly? end) { if (end < start) throw new ArgumentException("End date cannot precede start date."); StartDate=start; EndDate=end; }
}
