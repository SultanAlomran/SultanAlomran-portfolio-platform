using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class UserRatingConfiguration : IEntityTypeConfiguration<UserRating>
{
    public void Configure(EntityTypeBuilder<UserRating> builder)
    {
        builder.ConfigureCommon("UserRatings");
        builder.HasIndex(x => new { x.UserId, x.EntityType, x.EntityId }).IsUnique(); builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade); builder.ToTable(t => t.HasCheckConstraint("CK_UserRatings_Rating", "[Rating] BETWEEN 1 AND 5"));
    }
}
