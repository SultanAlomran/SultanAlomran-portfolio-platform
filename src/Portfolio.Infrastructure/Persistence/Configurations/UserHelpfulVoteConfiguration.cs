using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class UserHelpfulVoteConfiguration : IEntityTypeConfiguration<UserHelpfulVote>
{
    public void Configure(EntityTypeBuilder<UserHelpfulVote> builder)
    {
        builder.ConfigureCommon("UserHelpfulVotes");
        builder.HasIndex(x => new { x.UserId, x.EntityType, x.EntityId }).IsUnique(); builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
    }
}
