using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class CertificationConfiguration : IEntityTypeConfiguration<Certification>
{
    public void Configure(EntityTypeBuilder<Certification> builder)
    {
        builder.ConfigureCommon("Certifications");
        builder.HasIndex(x => new { x.Name, x.Issuer }).IsUnique(); builder.HasOne(x => x.MediaFile).WithMany().HasForeignKey(x => x.MediaFileId).OnDelete(DeleteBehavior.Restrict); builder.ToTable(t => t.HasCheckConstraint("CK_Certifications_DateRange", "[ExpiresDate] IS NULL OR [ExpiresDate] >= [IssuedDate]"));
    }
}
