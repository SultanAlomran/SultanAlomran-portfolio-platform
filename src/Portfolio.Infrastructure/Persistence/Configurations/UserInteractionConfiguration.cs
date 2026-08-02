using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class UserInteractionConfiguration : IEntityTypeConfiguration<UserInteraction>
{
    public void Configure(EntityTypeBuilder<UserInteraction> builder)
    {
        builder.ConfigureCommon("UserInteractions");
        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.InteractionType, x.CreatedAt }); builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}
