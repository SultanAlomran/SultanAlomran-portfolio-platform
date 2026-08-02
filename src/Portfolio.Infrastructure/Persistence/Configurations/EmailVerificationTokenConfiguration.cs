using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.ConfigureCommon("EmailVerificationTokens");
        builder.HasIndex(x => x.TokenHash).IsUnique(); builder.HasIndex(x => new { x.UserId, x.ExpiresAt }); builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
    }
}
