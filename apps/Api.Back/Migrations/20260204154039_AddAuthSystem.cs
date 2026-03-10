using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Back.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PREFERENCES",
                columns: table => new
                {
                    id_preferences = table.Column<Guid>(type: "uuid", nullable: false),
                    theme_preferences = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    appearence_preferences = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    theme_auto_preferences = table.Column<bool>(type: "boolean", nullable: false),
                    font_size_preferences = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                    letter_spacing_preferences = table.Column<decimal>(type: "numeric(15,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PREFERENCES", x => x.id_preferences);
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    id_users = table.Column<Guid>(type: "uuid", nullable: false),
                    name_users = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    firstname_users = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email_users = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email_verified_users = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash_users = table.Column<string>(type: "text", nullable: true),
                    title_users = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    experience_users = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    current_workload_percentage_users = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    workload_point_users = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    creation_date_users = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date_users = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    isDeleted_users = table.Column<bool>(type: "boolean", nullable: false),
                    id_preferences = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.id_users);
                    table.ForeignKey(
                        name: "FK_USERS_PREFERENCES_id_preferences",
                        column: x => x.id_preferences,
                        principalTable: "PREFERENCES",
                        principalColumn: "id_preferences",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users_credentials",
                columns: table => new
                {
                    id_users_crendentials = table.Column<Guid>(type: "uuid", nullable: false),
                    descriptor_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    public_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    user_handle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    signature_counter = table.Column<long>(type: "bigint", nullable: false),
                    aa_guid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    device_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    id_users = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users_credentials", x => x.id_users_crendentials);
                    table.ForeignKey(
                        name: "FK_users_credentials_USERS_id_users",
                        column: x => x.id_users,
                        principalTable: "USERS",
                        principalColumn: "id_users",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_USERS_id_preferences",
                table: "USERS",
                column: "id_preferences",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_credentials_id_users",
                table: "users_credentials",
                column: "id_users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users_credentials");

            migrationBuilder.DropTable(
                name: "USERS");

            migrationBuilder.DropTable(
                name: "PREFERENCES");
        }
    }
}
