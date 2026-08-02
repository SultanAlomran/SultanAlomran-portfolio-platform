using Portfolio.Domain.Common;
using Portfolio.Domain.Enums;

namespace Portfolio.Domain.Entities;

public sealed class SiteSetting : Entity
{
    private SiteSetting() { }
    public string SettingKey { get; private set; } = "";
    public string? SettingValue { get; private set; }
    public string? Description { get; private set; }
    public bool IsEncrypted { get; private set; }
    public DateTime? UpdatedAt { get; private set; }


}
