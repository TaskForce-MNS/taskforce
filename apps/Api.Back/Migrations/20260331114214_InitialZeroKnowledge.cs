using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Back.Migrations
{
    /// <inheritdoc />
    public partial class InitialZeroKnowledge : Migration
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
                name: "IDENTITIES",
                columns: table => new
                {
                    id_identity = table.Column<Guid>(type: "uuid", nullable: false),
                    encrypted_profile_blob = table.Column<byte[]>(type: "bytea", nullable: false),
                    experience_identity = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    title_identity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    current_workload_percentage_identity = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    workload_point_identity = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    creation_date_identity = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    update_date_identity = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    isDeleted_identity = table.Column<bool>(type: "boolean", nullable: false),
                    id_preferences = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IDENTITIES", x => x.id_identity);
                    table.ForeignKey(
                        name: "FK_IDENTITIES_PREFERENCES_id_preferences",
                        column: x => x.id_preferences,
                        principalTable: "PREFERENCES",
                        principalColumn: "id_preferences",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "USER_CREDENTIALS",
                columns: table => new
                {
                    id_credential = table.Column<Guid>(type: "uuid", nullable: false),
                    descriptor_id = table.Column<byte[]>(type: "bytea", nullable: false),
                    public_key = table.Column<byte[]>(type: "bytea", nullable: false),
                    user_handle = table.Column<byte[]>(type: "bytea", nullable: false),
                    signature_counter = table.Column<long>(type: "bigint", nullable: false),
                    aa_guid = table.Column<Guid>(type: "uuid", nullable: true),
                    device_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_identity = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_CREDENTIALS", x => x.id_credential);
                    table.ForeignKey(
                        name: "FK_USER_CREDENTIALS_IDENTITIES_id_identity",
                        column: x => x.id_identity,
                        principalTable: "IDENTITIES",
                        principalColumn: "id_identity",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IDENTITIES_id_preferences",
                table: "IDENTITIES",
                column: "id_preferences",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_CREDENTIALS_id_identity",
                table: "USER_CREDENTIALS",
                column: "id_identity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "USER_CREDENTIALS");

            migrationBuilder.DropTable(
                name: "IDENTITIES");

            migrationBuilder.DropTable(
                name: "PREFERENCES");
        }
    }
}
