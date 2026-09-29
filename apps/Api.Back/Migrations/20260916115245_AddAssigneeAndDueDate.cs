using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Back.Migrations
{
    /// <inheritdoc />
    public partial class AddAssigneeAndDueDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "assignee_id",
                table: "tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "due_date",
                table: "tasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tasks_assignee_id",
                table: "tasks",
                column: "assignee_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_IDENTITIES_assignee_id",
                table: "tasks",
                column: "assignee_id",
                principalTable: "IDENTITIES",
                principalColumn: "id_identity",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tasks_IDENTITIES_assignee_id",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "IX_tasks_assignee_id",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "assignee_id",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "due_date",
                table: "tasks");
        }
    }
}
