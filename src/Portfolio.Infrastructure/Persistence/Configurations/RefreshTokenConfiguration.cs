using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ConfigureCommon("RefreshTokens");
        builder.HasIndex(x => x.TokenHash).IsUnique(); builder.HasIndex(x => new { x.UserId, x.ExpiresAt }); builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.ReplacedByToken).WithMany().HasForeignKey(x => x.ReplacedByTokenId).OnDelete(DeleteBehavior.NoAction);
    }
}
