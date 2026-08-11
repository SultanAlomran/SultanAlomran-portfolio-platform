using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InfographicMediaAndFeatured : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CoverMediaFileId",
                table: "Infographics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InfographicMediaFileId",
                table: "Infographics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Infographics",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PdfMediaFileId",
                table: "Infographics",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Infographics_CoverMediaFileId",
                table: "Infographics",
                column: "CoverMediaFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Infographics_InfographicMediaFileId",
                table: "Infographics",
                column: "InfographicMediaFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Infographics_IsFeatured_Status_PublishedAt",
                table: "Infographics",
                columns: new[] { "IsFeatured", "Status", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Infographics_PdfMediaFileId",
                table: "Infographics",
                column: "PdfMediaFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Infographics_MediaFiles_CoverMediaFileId",
                table: "Infographics",
                column: "CoverMediaFileId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Infographics_MediaFiles_InfographicMediaFileId",
                table: "Infographics",
                column: "InfographicMediaFileId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Infographics_MediaFiles_PdfMediaFileId",
                table: "Infographics",
                column: "PdfMediaFileId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Infographics_MediaFiles_CoverMediaFileId",
                table: "Infographics");

            migrationBuilder.DropForeignKey(
                name: "FK_Infographics_MediaFiles_InfographicMediaFileId",
                table: "Infographics");

            migrationBuilder.DropForeignKey(
                name: "FK_Infographics_MediaFiles_PdfMediaFileId",
                table: "Infographics");

            migrationBuilder.DropIndex(
                name: "IX_Infographics_CoverMediaFileId",
                table: "Infographics");

            migrationBuilder.DropIndex(
                name: "IX_Infographics_InfographicMediaFileId",
                table: "Infographics");

            migrationBuilder.DropIndex(
                name: "IX_Infographics_IsFeatured_Status_PublishedAt",
                table: "Infographics");

            migrationBuilder.DropIndex(
                name: "IX_Infographics_PdfMediaFileId",
                table: "Infographics");

            migrationBuilder.DropColumn(
                name: "CoverMediaFileId",
                table: "Infographics");

            migrationBuilder.DropColumn(
                name: "InfographicMediaFileId",
                table: "Infographics");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Infographics");

            migrationBuilder.DropColumn(
                name: "PdfMediaFileId",
                table: "Infographics");
        }
    }
}
