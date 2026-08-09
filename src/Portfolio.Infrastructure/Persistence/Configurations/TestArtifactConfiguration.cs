using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class TestArtifactConfiguration : IEntityTypeConfiguration<TestArtifact>
{
    public void Configure(EntityTypeBuilder<TestArtifact> builder)
    {
        builder.ConfigureCommon("TestArtifacts");
        builder.Property(x => x.ProviderArtifactId).HasMaxLength(160).HasColumnType("nvarchar(160)");
        builder.Property(x => x.Name).HasMaxLength(500).HasColumnType("nvarchar(500)");
        builder.Property(x => x.MimeType).HasMaxLength(160).HasColumnType("nvarchar(160)");
        builder.Property(x => x.ExternalUrl).HasMaxLength(2048).HasColumnType("nvarchar(2048)");
        builder.Property(x => x.StoragePath).HasMaxLength(2048).HasColumnType("nvarchar(2048)");
        builder.Property(x => x.Browser).HasMaxLength(80).HasColumnType("nvarchar(80)");
        builder.Property(x => x.Feature).HasMaxLength(160).HasColumnType("nvarchar(160)");
        builder.HasOne(x => x.TestRun).WithMany(x => x.Artifacts).HasForeignKey(x => x.TestRunId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.TestCaseResult).WithMany(x => x.Artifacts).HasForeignKey(x => x.TestCaseResultId).OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.TestRunId);
        builder.HasIndex(x => x.TestCaseResultId);
        builder.HasIndex(x => new { x.Provider, x.ProviderArtifactId });
        builder.HasIndex(x => new { x.AvailabilityStatus, x.ExpiresAtUtc });
    }
}
