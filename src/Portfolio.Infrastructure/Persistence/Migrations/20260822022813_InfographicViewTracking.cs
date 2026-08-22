using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InfographicViewTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InfographicViews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    InfographicId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitorKeyHash = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfographicViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InfographicViews_Infographics_InfographicId",
                        column: x => x.InfographicId,
                        principalTable: "Infographics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InfographicViews_InfographicId_CreatedAt",
                table: "InfographicViews",
                columns: new[] { "InfographicId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_InfographicViews_Visitor_Infographic_CreatedAt",
                table: "InfographicViews",
                columns: new[] { "VisitorKeyHash", "InfographicId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InfographicViews");
        }
    }
}
