using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Back.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFirstAndLastName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Lastname_identity",
                table: "IDENTITIES",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "firstname_identity",
                table: "IDENTITIES",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Lastname_identity",
                table: "IDENTITIES");

            migrationBuilder.DropColumn(
                name: "firstname_identity",
                table: "IDENTITIES");
        }
    }
}
