using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Back.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "invitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    uses_left = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitations", x => x.id);
                    table.ForeignKey(
                        name: "FK_invitations_IDENTITIES_created_by",
                        column: x => x.created_by,
                        principalTable: "IDENTITIES",
                        principalColumn: "id_identity",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invitations_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id_project",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_invitations_created_by",
                table: "invitations",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_expires_at",
                table: "invitations",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_project_id",
                table: "invitations",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_token",
                table: "invitations",
                column: "token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invitations");
        }
    }
}
