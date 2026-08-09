using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class TestCaseResultConfiguration : IEntityTypeConfiguration<TestCaseResult>
{
    public void Configure(EntityTypeBuilder<TestCaseResult> builder)
    {
        builder.ConfigureCommon("TestCaseResults");
        builder.Property(x => x.Feature).HasMaxLength(160).HasColumnType("nvarchar(160)");
        builder.Property(x => x.Suite).HasMaxLength(500).HasColumnType("nvarchar(500)");
        builder.Property(x => x.TestName).HasMaxLength(500).HasColumnType("nvarchar(500)");
        builder.Property(x => x.ProjectArea).HasMaxLength(100).HasColumnType("nvarchar(100)");
        builder.Property(x => x.Browser).HasMaxLength(80).HasColumnType("nvarchar(80)");
        builder.Property(x => x.Viewport).HasMaxLength(40).HasColumnType("nvarchar(40)");
        builder.Property(x => x.ErrorType).HasMaxLength(160).HasColumnType("nvarchar(160)");
        builder.Property(x => x.ErrorSummary).HasMaxLength(2000).HasColumnType("nvarchar(2000)");
        builder.Property(x => x.SourceFile).HasMaxLength(1000).HasColumnType("nvarchar(1000)");
        builder.HasOne(x => x.TestRun).WithMany(x => x.TestCaseResults).HasForeignKey(x => x.TestRunId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.TestRunId);
        builder.HasIndex(x => x.TestName);
        builder.HasIndex(x => x.Browser);
        builder.HasIndex(x => x.Feature);
        builder.HasIndex(x => x.IsFlaky);
    }
}
