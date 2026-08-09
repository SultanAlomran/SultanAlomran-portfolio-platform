using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TestAnalyticsFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    ProviderRunId = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    WorkflowName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WorkflowRunNumber = table.Column<long>(type: "bigint", nullable: true),
                    Branch = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CommitSha = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PullRequestNumber = table.Column<int>(type: "int", nullable: true),
                    Trigger = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ExecutionMode = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    PassedCount = table.Column<int>(type: "int", nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    SkippedCount = table.Column<int>(type: "int", nullable: false),
                    FlakyCount = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    BrowserSummary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FeatureSummary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TotalScreenshots = table.Column<int>(type: "int", nullable: false),
                    TotalVideos = table.Column<int>(type: "int", nullable: false),
                    TotalTraces = table.Column<int>(type: "int", nullable: false),
                    TotalReports = table.Column<int>(type: "int", nullable: false),
                    RepositoryUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    WorkflowRunUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    PullRequestUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestCaseResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    TestRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Feature = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Suite = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TestName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProjectArea = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Browser = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Viewport = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    IsFlaky = table.Column<bool>(type: "bit", nullable: false),
                    ErrorType = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    ErrorSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SourceFile = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestCaseResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestCaseResults_TestRuns_TestRunId",
                        column: x => x.TestRunId,
                        principalTable: "TestRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestArtifacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    TestRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestCaseResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArtifactType = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<int>(type: "int", nullable: false),
                    ProviderArtifactId = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    ExternalUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    StoragePath = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AvailabilityStatus = table.Column<int>(type: "int", nullable: false),
                    Browser = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Feature = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestArtifacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestArtifacts_TestCaseResults_TestCaseResultId",
                        column: x => x.TestCaseResultId,
                        principalTable: "TestCaseResults",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestArtifacts_TestRuns_TestRunId",
                        column: x => x.TestRunId,
                        principalTable: "TestRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestArtifacts_AvailabilityStatus_ExpiresAtUtc",
                table: "TestArtifacts",
                columns: new[] { "AvailabilityStatus", "ExpiresAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TestArtifacts_Provider_ProviderArtifactId",
                table: "TestArtifacts",
                columns: new[] { "Provider", "ProviderArtifactId" });

            migrationBuilder.CreateIndex(
                name: "IX_TestArtifacts_TestCaseResultId",
                table: "TestArtifacts",
                column: "TestCaseResultId");

            migrationBuilder.CreateIndex(
                name: "IX_TestArtifacts_TestRunId",
                table: "TestArtifacts",
                column: "TestRunId");

            migrationBuilder.CreateIndex(
                name: "IX_TestCaseResults_Browser",
                table: "TestCaseResults",
                column: "Browser");

            migrationBuilder.CreateIndex(
                name: "IX_TestCaseResults_Feature",
                table: "TestCaseResults",
                column: "Feature");

            migrationBuilder.CreateIndex(
                name: "IX_TestCaseResults_IsFlaky",
                table: "TestCaseResults",
                column: "IsFlaky");

            migrationBuilder.CreateIndex(
                name: "IX_TestCaseResults_TestName",
                table: "TestCaseResults",
                column: "TestName");

            migrationBuilder.CreateIndex(
                name: "IX_TestCaseResults_TestRunId",
                table: "TestCaseResults",
                column: "TestRunId");

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_Branch",
                table: "TestRuns",
                column: "Branch");

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_CommitSha",
                table: "TestRuns",
                column: "CommitSha");

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_ExecutionMode",
                table: "TestRuns",
                column: "ExecutionMode");

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_Provider_ProviderRunId",
                table: "TestRuns",
                columns: new[] { "Provider", "ProviderRunId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_PullRequestNumber",
                table: "TestRuns",
                column: "PullRequestNumber");

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_StartedAtUtc",
                table: "TestRuns",
                column: "StartedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TestRuns_Status",
                table: "TestRuns",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestArtifacts");

            migrationBuilder.DropTable(
                name: "TestCaseResults");

            migrationBuilder.DropTable(
                name: "TestRuns");
        }
    }
}
