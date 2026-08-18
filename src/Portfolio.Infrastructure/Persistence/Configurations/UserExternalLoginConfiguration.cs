using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class UserExternalLoginConfiguration : IEntityTypeConfiguration<UserExternalLogin>
{
    public void Configure(EntityTypeBuilder<UserExternalLogin> builder)
    {
        builder.ConfigureCommon("UserExternalLogins");
        builder.Property(x => x.Provider).HasMaxLength(100).HasColumnType("nvarchar(100)");
        builder.Property(x => x.ProviderSubject).HasMaxLength(255).HasColumnType("nvarchar(255)");
        builder.Property(x => x.ProviderEmail).HasMaxLength(320).HasColumnType("nvarchar(320)");
        builder.HasIndex(x => new { x.Provider, x.ProviderSubject }).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.Provider }).IsUnique();
        builder.HasOne(x => x.User).WithMany(x => x.ExternalLogins).HasForeignKey(x => x.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
    }
}
