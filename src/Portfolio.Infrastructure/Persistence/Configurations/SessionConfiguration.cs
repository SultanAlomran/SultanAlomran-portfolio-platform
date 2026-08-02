using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ConfigureCommon("Sessions");
        builder.HasIndex(x => x.SessionIdentifier).IsUnique(); builder.HasIndex(x => x.StartedAt); builder.HasOne(x => x.User).WithMany(x => x.Sessions).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull); builder.ToTable(t => t.HasCheckConstraint("CK_Sessions_DateRange", "[EndedAt] IS NULL OR [EndedAt] >= [StartedAt]"));
    }
}
