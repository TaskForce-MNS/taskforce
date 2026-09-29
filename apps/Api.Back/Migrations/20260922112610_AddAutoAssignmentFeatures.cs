using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Back.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoAssignmentFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "difficulty",
                table: "tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<List<string>>(
                name: "required_domains",
                table: "tasks",
                type: "text[]",
                nullable: false, 
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<int>(
                name: "story_points",
                table: "tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "target_week",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "user_skills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Domain = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    DbIdentityId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_skills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_skills_IDENTITIES_DbIdentityId",
                        column: x => x.DbIdentityId,
                        principalTable: "IDENTITIES",
                        principalColumn: "id_identity");
                    table.ForeignKey(
                        name: "FK_user_skills_IDENTITIES_UserId",
                        column: x => x.UserId,
                        principalTable: "IDENTITIES",
                        principalColumn: "id_identity",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_skills_DbIdentityId",
                table: "user_skills",
                column: "DbIdentityId");

            migrationBuilder.CreateIndex(
                name: "IX_user_skills_UserId",
                table: "user_skills",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_skills");

            migrationBuilder.DropColumn(
                name: "difficulty",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "required_domains",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "story_points",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "target_week",
                table: "tasks");
        }
    }
}
