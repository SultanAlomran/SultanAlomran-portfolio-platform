using Microsoft.EntityFrameworkCore;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Seed;

internal static class ReferenceDataSeed
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        var administratorRoleId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        modelBuilder.Entity<Role>().HasData(new { Id = administratorRoleId, Name = "Administrator", Description = "Full administrative role; no account or credential is seeded." });
        modelBuilder.Entity<Permission>().HasData(
            new { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), Name = "content.manage", Description = "Manage portfolio content." },
            new { Id = Guid.Parse("20000000-0000-0000-0000-000000000002"), Name = "settings.manage", Description = "Manage non-secret site settings." },
            new { Id = Guid.Parse("20000000-0000-0000-0000-000000000003"), Name = "analytics.read", Description = "Read approved aggregate analytics." });
        modelBuilder.Entity<RolePermission>().HasData(
            new { Id = Guid.Parse("30000000-0000-0000-0000-000000000001"), RoleId = administratorRoleId, PermissionId = Guid.Parse("20000000-0000-0000-0000-000000000001") },
            new { Id = Guid.Parse("30000000-0000-0000-0000-000000000002"), RoleId = administratorRoleId, PermissionId = Guid.Parse("20000000-0000-0000-0000-000000000002") },
            new { Id = Guid.Parse("30000000-0000-0000-0000-000000000003"), RoleId = administratorRoleId, PermissionId = Guid.Parse("20000000-0000-0000-0000-000000000003") });
        modelBuilder.Entity<SiteSetting>().HasData(new { Id = Guid.Parse("40000000-0000-0000-0000-000000000001"), SettingKey = "site.defaultCulture", SettingValue = "ar-SA", Description = "Default presentation culture.", IsEncrypted = false });
    }
}
