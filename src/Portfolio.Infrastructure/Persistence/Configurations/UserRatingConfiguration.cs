using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class UserRatingConfiguration : IEntityTypeConfiguration<UserRating>
{
    public void Configure(EntityTypeBuilder<UserRating> builder)
    {
        builder.ConfigureCommon("UserRatings");
        builder.Property(x => x.VisitorKeyHash).HasColumnType("char(64)").HasMaxLength(64).IsUnicode(false);
        builder.HasIndex(x => new { x.UserId, x.EntityType, x.EntityId }).IsUnique().HasFilter("[UserId] IS NOT NULL");
        builder.HasIndex(x => new { x.VisitorKeyHash, x.EntityType, x.EntityId }).IsUnique().HasFilter("[VisitorKeyHash] IS NOT NULL");
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_UserRatings_Actor", "([UserId] IS NOT NULL AND [VisitorKeyHash] IS NULL) OR ([UserId] IS NULL AND [VisitorKeyHash] IS NOT NULL)");
            t.HasCheckConstraint("CK_UserRatings_Rating", "[Rating] BETWEEN 1 AND 5");
        });
    }
}
