using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        builder.ConfigureCommon("Profiles");
        builder.HasIndex(x => x.SingletonKey).IsUnique(); builder.HasOne(x => x.ProfileImageMediaFile).WithMany().HasForeignKey(x => x.ProfileImageMediaFileId).OnDelete(DeleteBehavior.Restrict); builder.HasOne(x => x.CvMediaFile).WithMany().HasForeignKey(x => x.CvMediaFileId).OnDelete(DeleteBehavior.Restrict);
    }
}
