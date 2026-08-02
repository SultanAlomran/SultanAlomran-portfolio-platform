using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class PageViewConfiguration : IEntityTypeConfiguration<PageView>
{
    public void Configure(EntityTypeBuilder<PageView> builder)
    {
        builder.ConfigureCommon("PageViews");
        builder.HasIndex(x => x.CreatedAt); builder.HasIndex(x => x.Url); builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.NoAction); builder.HasOne(x => x.Session).WithMany(x => x.PageViews).HasForeignKey(x => x.SessionId).OnDelete(DeleteBehavior.SetNull);
    }
}
