using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class PopularSearch : Entity
{
    private PopularSearch() { }
    public string SearchTerm { get; private set; } = "";
    public int SearchCount { get; private set; }

    
}
