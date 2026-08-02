using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Common;
using Portfolio.Domain.Constants;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal static class ConfigurationExtensions
{
    private static readonly Dictionary<string, int> Lengths = new(StringComparer.Ordinal)
    {
        ["UserName"] = DatabaseLengths.UserName,
        ["Email"] = DatabaseLengths.Email,
        ["FullName"] = DatabaseLengths.FullName,
        ["PasswordHash"] = DatabaseLengths.PasswordHash,
        ["Name"] = DatabaseLengths.Name,
        ["Issuer"] = DatabaseLengths.CertificateIssuer,
        ["JobTitle"] = DatabaseLengths.JobTitle,
        ["OrganizationName"] = DatabaseLengths.OrganizationName,
        ["Title"] = DatabaseLengths.Title,
        ["Subject"] = DatabaseLengths.Title,
        ["Slug"] = DatabaseLengths.Slug,
        ["ShortDescription"] = DatabaseLengths.ShortDescription,
        ["Url"] = DatabaseLengths.Url,
        ["LiveUrl"] = DatabaseLengths.Url,
        ["WebsiteUrl"] = DatabaseLengths.Url,
        ["VerificationUrl"] = DatabaseLengths.Url,
        ["PageRoute"] = DatabaseLengths.Url,
        ["Referrer"] = DatabaseLengths.Url,
        ["FileName"] = DatabaseLengths.FileName,
        ["OriginalFileName"] = DatabaseLengths.FileName,
        ["FilePath"] = DatabaseLengths.FilePath,
        ["MimeType"] = DatabaseLengths.MimeType,
        ["Icon"] = DatabaseLengths.Icon,
        ["EntityType"] = DatabaseLengths.EntityType,
        ["IpAddress"] = DatabaseLengths.IpAddress,
        ["CreatedByIp"] = DatabaseLengths.IpAddress,
        ["ReplacedByIp"] = DatabaseLengths.IpAddress,
        ["IpUsed"] = DatabaseLengths.IpAddress,
        ["IpUsedFrom"] = DatabaseLengths.IpAddress,
        ["UserAgent"] = DatabaseLengths.UserAgent,
        ["TokenHash"] = DatabaseLengths.TokenHash,
        ["SessionIdentifier"] = DatabaseLengths.Name,
        ["AltText"] = DatabaseLengths.ShortDescription,
        ["Caption"] = DatabaseLengths.ShortDescription,
        ["Checksum"] = 128,
        ["StorageProvider"] = 100,
        ["Country"] = 100,
        ["Device"] = DatabaseLengths.ShortName,
        ["Browser"] = DatabaseLengths.ShortName,
        ["Action"] = DatabaseLengths.ShortName,
        ["Language"] = 100,
        ["LinkType"] = 100,
        ["ResourceType"] = 100,
        ["Category"] = 100,
        ["CredentialId"] = DatabaseLengths.VerificationCode,
        ["SingletonKey"] = 20,
        ["Headline"] = DatabaseLengths.Title,
        ["Location"] = DatabaseLengths.Name,
        ["SearchTerm"] = 300,
        ["SettingKey"] = DatabaseLengths.Name
    };

    public static void ConfigureCommon<TEntity>(this EntityTypeBuilder<TEntity> builder, string table)
        where TEntity : Entity
    {
        builder.ToTable(table);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");
        foreach (var property in typeof(TEntity).GetProperties().Where(x => x.PropertyType == typeof(string) || x.PropertyType == typeof(string)))
        {
            var configured = builder.Property(property.Name).IsUnicode().HasColumnType("nvarchar(max)");
            if (Lengths.TryGetValue(property.Name, out var length))
                configured.HasMaxLength(length).HasColumnType($"nvarchar({length})");
        }
        foreach (var property in typeof(TEntity).GetProperties().Where(x => x.PropertyType == typeof(DateTime) || x.PropertyType == typeof(DateTime?)))
            builder.Property(property.Name).HasColumnType("datetime2");
        foreach (var property in typeof(TEntity).GetProperties().Where(x => x.PropertyType == typeof(DateOnly) || x.PropertyType == typeof(DateOnly?)))
            builder.Property(property.Name).HasColumnType("date");
    }
}
