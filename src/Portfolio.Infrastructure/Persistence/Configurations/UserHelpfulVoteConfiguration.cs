using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class UserHelpfulVoteConfiguration : IEntityTypeConfiguration<UserHelpfulVote>
{
    public void Configure(EntityTypeBuilder<UserHelpfulVote> builder)
    {
        builder.ConfigureCommon("UserHelpfulVotes");
        builder.Property(x => x.VisitorKeyHash).HasColumnType("char(64)").HasMaxLength(64).IsUnicode(false);
        builder.HasIndex(x => new { x.UserId, x.EntityType, x.EntityId }).IsUnique().HasFilter("[UserId] IS NOT NULL");
        builder.HasIndex(x => new { x.VisitorKeyHash, x.EntityType, x.EntityId }).IsUnique().HasFilter("[VisitorKeyHash] IS NOT NULL");
        builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_UserHelpfulVotes_Actor", "([UserId] IS NOT NULL AND [VisitorKeyHash] IS NULL) OR ([UserId] IS NULL AND [VisitorKeyHash] IS NOT NULL)");
            t.HasCheckConstraint("CK_UserHelpfulVotes_NegativeReason", "([IsHelpful] = 1 AND [NegativeFeedbackReason] IS NULL) OR ([IsHelpful] = 0 AND ([NegativeFeedbackReason] IS NULL OR [NegativeFeedbackReason] BETWEEN 1 AND 7))");
        });
    }
}
