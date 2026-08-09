using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Persistence.Configurations;

internal sealed class TestRunConfiguration : IEntityTypeConfiguration<TestRun>
{
    public void Configure(EntityTypeBuilder<TestRun> builder)
    {
        builder.ConfigureCommon("TestRuns");
        builder.Property(x => x.ProviderRunId).HasMaxLength(160).HasColumnType("nvarchar(160)");
        builder.Property(x => x.WorkflowName).HasMaxLength(200).HasColumnType("nvarchar(200)");
        builder.Property(x => x.Branch).HasMaxLength(250).HasColumnType("nvarchar(250)");
        builder.Property(x => x.CommitSha).HasMaxLength(64).HasColumnType("nvarchar(64)");
        builder.Property(x => x.Trigger).HasMaxLength(80).HasColumnType("nvarchar(80)");
        builder.Property(x => x.BrowserSummary).HasMaxLength(1000).HasColumnType("nvarchar(1000)");
        builder.Property(x => x.FeatureSummary).HasMaxLength(1000).HasColumnType("nvarchar(1000)");
        builder.Property(x => x.RepositoryUrl).HasMaxLength(2048).HasColumnType("nvarchar(2048)");
        builder.Property(x => x.WorkflowRunUrl).HasMaxLength(2048).HasColumnType("nvarchar(2048)");
        builder.Property(x => x.PullRequestUrl).HasMaxLength(2048).HasColumnType("nvarchar(2048)");
        builder.HasIndex(x => new { x.Provider, x.ProviderRunId }).IsUnique();
        builder.HasIndex(x => x.StartedAtUtc);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Branch);
        builder.HasIndex(x => x.CommitSha);
        builder.HasIndex(x => x.PullRequestNumber);
        builder.HasIndex(x => x.ExecutionMode);
    }
}
