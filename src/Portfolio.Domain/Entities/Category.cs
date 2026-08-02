using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class Category : SoftDeletableEntity
{
    private Category() { }
    public string Name { get; private set; } = "";
    public string Slug { get; private set; } = "";
    public string? Description { get; private set; }
    public Guid? ParentId { get; private set; }
    public string? Icon { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Category? Parent { get; private set; }
    public ICollection<Category> Children { get; private set; } = new HashSet<Category>();
    public ICollection<Infographic> Infographics { get; private set; } = new HashSet<Infographic>();
    
}
