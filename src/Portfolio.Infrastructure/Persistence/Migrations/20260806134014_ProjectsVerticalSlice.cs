using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProjectsVerticalSlice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Architecture",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessProblem",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Challenges",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Impact",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Projects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KeyFeatures",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LessonsLearned",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Solution",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsFeatured_Status_PublishedAt",
                table: "Projects",
                columns: new[] { "IsFeatured", "Status", "PublishedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_IsFeatured_Status_PublishedAt",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Architecture",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "BusinessProblem",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Challenges",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Impact",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "KeyFeatures",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "LessonsLearned",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Solution",
                table: "Projects");
        }
    }
}
