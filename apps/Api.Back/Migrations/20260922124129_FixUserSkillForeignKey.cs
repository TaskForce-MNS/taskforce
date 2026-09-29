using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Back.Migrations
{
    /// <inheritdoc />
    public partial class FixUserSkillForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_skills_IDENTITIES_DbIdentityId",
                table: "user_skills");

            migrationBuilder.DropIndex(
                name: "IX_user_skills_DbIdentityId",
                table: "user_skills");

            migrationBuilder.DropColumn(
                name: "DbIdentityId",
                table: "user_skills");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DbIdentityId",
                table: "user_skills",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_skills_DbIdentityId",
                table: "user_skills",
                column: "DbIdentityId");

            migrationBuilder.AddForeignKey(
                name: "FK_user_skills_IDENTITIES_DbIdentityId",
                table: "user_skills",
                column: "DbIdentityId",
                principalTable: "IDENTITIES",
                principalColumn: "id_identity");
        }
    }
}
