namespace Portfolio.Domain.Constants;

/// <summary>Approved limits for the initial SQL Server migration.</summary>
public static class DatabaseLengths
{
    public const int UserName = 100, Email = 320, FullName = 200, PasswordHash = 500;
    public const int ShortName = 150, Name = 200, Title = 250, Slug = 200, ShortDescription = 500;
    public const int Url = 2048, FileName = 255, FilePath = 1000, MimeType = 150, Icon = 200;
    public const int EntityType = 100, InteractionType = 50, IpAddress = 45, UserAgent = 1000;
    public const int TokenHash = 500, CertificateIssuer = 250, VerificationCode = 500;
    public const int JobTitle = 250, OrganizationName = 250;
}
