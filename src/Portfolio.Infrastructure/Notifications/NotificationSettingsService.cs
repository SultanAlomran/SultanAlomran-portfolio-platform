using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Portfolio.Application.Notifications;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure.Persistence;

namespace Portfolio.Infrastructure.Notifications;

public sealed class NotificationSettingsService(
    PortfolioDbContext db,
    IOptions<NotificationOptions> options) : INotificationSettingsService
{
    private const string EmailEnabledKey = "Notification.Email.Enabled";
    private const string WhatsAppEnabledKey = "Notification.WhatsApp.Enabled";
    private const string AdminToastEnabledKey = "Notification.AdminToast.Enabled";

    public async Task<NotificationSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var emailEnabled = await IsEmailEnabledAsync(cancellationToken);
        var waEnabled = await IsWhatsAppEnabledAsync(cancellationToken);
        var toastEnabled = await IsAdminToastEnabledAsync(cancellationToken);

        return new NotificationSettingsDto(
            EmailEnabled: emailEnabled,
            WhatsAppEnabled: waEnabled,
            AdminToastEnabled: toastEnabled,
            EmailProvider: options.Value.Email.Provider,
            WhatsAppProvider: options.Value.WhatsApp.Provider,
            RecipientEmail: options.Value.Email.RecipientEmail,
            RecipientPhoneNumber: options.Value.WhatsApp.RecipientPhoneNumber);
    }

    public async Task<NotificationSettingsDto> UpdateSettingsAsync(UpdateNotificationSettingsRequest request, CancellationToken cancellationToken = default)
    {
        await SetSettingValueAsync(EmailEnabledKey, request.EmailEnabled.ToString().ToLowerInvariant(), "Email notifications enabled", cancellationToken);
        await SetSettingValueAsync(WhatsAppEnabledKey, request.WhatsAppEnabled.ToString().ToLowerInvariant(), "WhatsApp notifications enabled", cancellationToken);
        await SetSettingValueAsync(AdminToastEnabledKey, request.AdminToastEnabled.ToString().ToLowerInvariant(), "Admin realtime toast alerts enabled", cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        return await GetSettingsAsync(cancellationToken);
    }

    public async Task<bool> IsEmailEnabledAsync(CancellationToken cancellationToken = default)
    {
        var setting = await db.SiteSettings.SingleOrDefaultAsync(x => x.SettingKey == EmailEnabledKey, cancellationToken);
        if (setting is not null && bool.TryParse(setting.SettingValue, out var enabled))
        {
            return enabled;
        }

        return options.Value.Email.Enabled;
    }

    public async Task<bool> IsWhatsAppEnabledAsync(CancellationToken cancellationToken = default)
    {
        var setting = await db.SiteSettings.SingleOrDefaultAsync(x => x.SettingKey == WhatsAppEnabledKey, cancellationToken);
        if (setting is not null && bool.TryParse(setting.SettingValue, out var enabled))
        {
            return enabled;
        }

        return options.Value.WhatsApp.Enabled;
    }

    public async Task<bool> IsAdminToastEnabledAsync(CancellationToken cancellationToken = default)
    {
        var setting = await db.SiteSettings.SingleOrDefaultAsync(x => x.SettingKey == AdminToastEnabledKey, cancellationToken);
        if (setting is not null && bool.TryParse(setting.SettingValue, out var enabled))
        {
            return enabled;
        }

        return options.Value.AdminToastEnabled;
    }

    private async Task SetSettingValueAsync(string key, string value, string description, CancellationToken cancellationToken)
    {
        var setting = await db.SiteSettings.SingleOrDefaultAsync(x => x.SettingKey == key, cancellationToken);
        if (setting is null)
        {
            setting = SiteSetting.Create(key, value, description);
            db.SiteSettings.Add(setting);
        }
        else
        {
            setting.SetValue(value);
        }
    }
}
