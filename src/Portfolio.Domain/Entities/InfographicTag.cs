using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class InfographicTag : Entity
{
    private InfographicTag() { }
    public Guid InfographicId { get; private set; }
    public Guid TagId { get; private set; }
    public Infographic Infographic { get; private set; } = null!;
    public Tag Tag { get; private set; } = null!;

}
