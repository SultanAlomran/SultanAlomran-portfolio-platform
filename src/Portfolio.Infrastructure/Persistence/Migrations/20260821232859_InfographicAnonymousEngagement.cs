using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portfolio.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InfographicAnonymousEngagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRatings_UserId_EntityType_EntityId",
                table: "UserRatings");

            migrationBuilder.DropIndex(
                name: "IX_UserHelpfulVotes_UserId_EntityType_EntityId",
                table: "UserHelpfulVotes");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserRatings",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserRatings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisitorKeyHash",
                table: "UserRatings",
                type: "char(64)",
                unicode: false,
                maxLength: 64,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserHelpfulVotes",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<byte>(
                name: "NegativeFeedbackReason",
                table: "UserHelpfulVotes",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserHelpfulVotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisitorKeyHash",
                table: "UserHelpfulVotes",
                type: "char(64)",
                unicode: false,
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRatings_UserId_EntityType_EntityId",
                table: "UserRatings",
                columns: new[] { "UserId", "EntityType", "EntityId" },
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserRatings_VisitorKeyHash_EntityType_EntityId",
                table: "UserRatings",
                columns: new[] { "VisitorKeyHash", "EntityType", "EntityId" },
                unique: true,
                filter: "[VisitorKeyHash] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserRatings_Actor",
                table: "UserRatings",
                sql: "([UserId] IS NOT NULL AND [VisitorKeyHash] IS NULL) OR ([UserId] IS NULL AND [VisitorKeyHash] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_UserHelpfulVotes_UserId_EntityType_EntityId",
                table: "UserHelpfulVotes",
                columns: new[] { "UserId", "EntityType", "EntityId" },
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserHelpfulVotes_VisitorKeyHash_EntityType_EntityId",
                table: "UserHelpfulVotes",
                columns: new[] { "VisitorKeyHash", "EntityType", "EntityId" },
                unique: true,
                filter: "[VisitorKeyHash] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserHelpfulVotes_Actor",
                table: "UserHelpfulVotes",
                sql: "([UserId] IS NOT NULL AND [VisitorKeyHash] IS NULL) OR ([UserId] IS NULL AND [VisitorKeyHash] IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserHelpfulVotes_NegativeReason",
                table: "UserHelpfulVotes",
                sql: "([IsHelpful] = 1 AND [NegativeFeedbackReason] IS NULL) OR ([IsHelpful] = 0 AND ([NegativeFeedbackReason] IS NULL OR [NegativeFeedbackReason] BETWEEN 1 AND 7))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserRatings_UserId_EntityType_EntityId",
                table: "UserRatings");

            migrationBuilder.DropIndex(
                name: "IX_UserRatings_VisitorKeyHash_EntityType_EntityId",
                table: "UserRatings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserRatings_Actor",
                table: "UserRatings");

            migrationBuilder.DropIndex(
                name: "IX_UserHelpfulVotes_UserId_EntityType_EntityId",
                table: "UserHelpfulVotes");

            migrationBuilder.DropIndex(
                name: "IX_UserHelpfulVotes_VisitorKeyHash_EntityType_EntityId",
                table: "UserHelpfulVotes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserHelpfulVotes_Actor",
                table: "UserHelpfulVotes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserHelpfulVotes_NegativeReason",
                table: "UserHelpfulVotes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserRatings");

            migrationBuilder.DropColumn(
                name: "VisitorKeyHash",
                table: "UserRatings");

            migrationBuilder.DropColumn(
                name: "NegativeFeedbackReason",
                table: "UserHelpfulVotes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "UserHelpfulVotes");

            migrationBuilder.DropColumn(
                name: "VisitorKeyHash",
                table: "UserHelpfulVotes");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserRatings",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "UserHelpfulVotes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRatings_UserId_EntityType_EntityId",
                table: "UserRatings",
                columns: new[] { "UserId", "EntityType", "EntityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserHelpfulVotes_UserId_EntityType_EntityId",
                table: "UserHelpfulVotes",
                columns: new[] { "UserId", "EntityType", "EntityId" },
                unique: true);
        }
    }
}
