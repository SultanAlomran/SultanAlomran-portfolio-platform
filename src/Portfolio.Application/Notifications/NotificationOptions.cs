namespace Portfolio.Application.Notifications;

public sealed class NotificationOptions
{
    public const string SectionName = "Notifications";

    public EmailNotificationOptions Email { get; set; } = new();
    public WhatsAppNotificationOptions WhatsApp { get; set; } = new();
    public bool AdminToastEnabled { get; set; } = true;
    public string AdminBaseUrl { get; set; } = "http://localhost:4300";
}

public sealed class EmailNotificationOptions
{
    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "Deterministic";
    public string FromAddress { get; set; } = "DoNotReply@sultanomran.com";
    public string FromName { get; set; } = "Sultan Portfolio";
    public string RecipientEmail { get; set; } = "sultan.alomran.9@gmail.com";
    public string? ConnectionString { get; set; }
    public string? SmtpHost { get; set; }
    public int SmtpPort { get; set; } = 587;
    public string? SmtpUser { get; set; }
    public string? SmtpPassword { get; set; }
}

public sealed class WhatsAppNotificationOptions
{
    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "Deterministic";
    public string RecipientPhoneNumber { get; set; } = "+966508334411";
    public string ApiUrl { get; set; } = "https://graph.facebook.com/v21.0";
    public string? PhoneNumberId { get; set; }
    public string? AccessToken { get; set; }
    public string? ApiKey { get; set; }
}
