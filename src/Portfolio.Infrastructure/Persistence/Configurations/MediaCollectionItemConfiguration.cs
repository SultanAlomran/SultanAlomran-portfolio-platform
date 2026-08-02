using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class MediaCollectionItemConfiguration : IEntityTypeConfiguration<MediaCollectionItem>
{
    public void Configure(EntityTypeBuilder<MediaCollectionItem> builder)
    {
        builder.ConfigureCommon("MediaCollectionItems");
        builder.HasIndex(x => new { x.MediaCollectionId, x.MediaFileId }).IsUnique(); builder.HasOne(x => x.MediaCollection).WithMany(x => x.Items).HasForeignKey(x => x.MediaCollectionId).IsRequired().OnDelete(DeleteBehavior.Cascade); builder.HasOne(x => x.MediaFile).WithMany().HasForeignKey(x => x.MediaFileId).IsRequired().OnDelete(DeleteBehavior.Restrict); builder.ToTable(t => t.HasCheckConstraint("CK_MediaCollectionItems_DisplayOrder", "[DisplayOrder] >= 0"));
    }
}
