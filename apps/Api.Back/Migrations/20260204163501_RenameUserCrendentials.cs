using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Back.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserCrendentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_credentials_USERS_id_users",
                table: "users_credentials");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users_credentials",
                table: "users_credentials");

            migrationBuilder.RenameTable(
                name: "users_credentials",
                newName: "USERS_CREDENTIALS");

            migrationBuilder.RenameIndex(
                name: "IX_users_credentials_id_users",
                table: "USERS_CREDENTIALS",
                newName: "IX_USERS_CREDENTIALS_id_users");

            migrationBuilder.AddPrimaryKey(
                name: "PK_USERS_CREDENTIALS",
                table: "USERS_CREDENTIALS",
                column: "id_users_crendentials");

            migrationBuilder.AddForeignKey(
                name: "FK_USERS_CREDENTIALS_USERS_id_users",
                table: "USERS_CREDENTIALS",
                column: "id_users",
                principalTable: "USERS",
                principalColumn: "id_users",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_USERS_CREDENTIALS_USERS_id_users",
                table: "USERS_CREDENTIALS");

            migrationBuilder.DropPrimaryKey(
                name: "PK_USERS_CREDENTIALS",
                table: "USERS_CREDENTIALS");

            migrationBuilder.RenameTable(
                name: "USERS_CREDENTIALS",
                newName: "users_credentials");

            migrationBuilder.RenameIndex(
                name: "IX_USERS_CREDENTIALS_id_users",
                table: "users_credentials",
                newName: "IX_users_credentials_id_users");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users_credentials",
                table: "users_credentials",
                column: "id_users_crendentials");

            migrationBuilder.AddForeignKey(
                name: "FK_users_credentials_USERS_id_users",
                table: "users_credentials",
                column: "id_users",
                principalTable: "USERS",
                principalColumn: "id_users",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
