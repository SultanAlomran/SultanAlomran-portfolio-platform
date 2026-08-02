using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class MediaFileConfiguration : IEntityTypeConfiguration<MediaFile>
{
    public void Configure(EntityTypeBuilder<MediaFile> builder)
    {
        builder.ConfigureCommon("MediaFiles");
        builder.HasIndex(x => x.FilePath).IsUnique(); builder.HasOne(x => x.Uploader).WithMany(x => x.UploadedMediaFiles).HasForeignKey(x => x.UploadedBy).OnDelete(DeleteBehavior.SetNull); builder.ToTable(t => { t.HasCheckConstraint("CK_MediaFiles_FileSize", "[FileSize] >= 0"); t.HasCheckConstraint("CK_MediaFiles_Width", "[Width] IS NULL OR [Width] > 0"); t.HasCheckConstraint("CK_MediaFiles_Height", "[Height] IS NULL OR [Height] > 0"); });
    }
}
