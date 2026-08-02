using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class UserHelpfulVoteConfiguration : IEntityTypeConfiguration<UserHelpfulVote>
{
    public void Configure(EntityTypeBuilder<UserHelpfulVote> builder)
    {
        builder.ConfigureCommon("UserHelpfulVotes");
        EntityRelationships.Configure(builder);
    }
}
