using Portfolio.Domain.Common;

namespace Portfolio.Domain.Entities;

public sealed class SiteSetting : Entity
{
    private SiteSetting() { }

    public string SettingKey { get; private set; } = "";
    public string? SettingValue { get; private set; }
    public string? Description { get; private set; }
    public bool IsEncrypted { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public static SiteSetting Create(string key, string? value, string? description = null, bool isEncrypted = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return new SiteSetting
        {
            SettingKey = key.Trim(),
            SettingValue = value,
            Description = description,
            IsEncrypted = isEncrypted,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void SetValue(string? value)
    {
        SettingValue = value;
        UpdatedAt = DateTime.UtcNow;
    }
}
