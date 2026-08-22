using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

public sealed class InfographicViewConfiguration : IEntityTypeConfiguration<InfographicView>
{
    public void Configure(EntityTypeBuilder<InfographicView> builder)
    {
        builder.ToTable("InfographicViews");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(x => x.VisitorKeyHash)
            .IsRequired()
            .HasMaxLength(64)
            .IsUnicode(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.Infographic)
            .WithMany(x => x.Views)
            .HasForeignKey(x => x.InfographicId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.InfographicId, x.CreatedAt })
            .HasDatabaseName("IX_InfographicViews_InfographicId_CreatedAt");

        builder.HasIndex(x => new { x.VisitorKeyHash, x.InfographicId, x.CreatedAt })
            .HasDatabaseName("IX_InfographicViews_Visitor_Infographic_CreatedAt");
    }
}
