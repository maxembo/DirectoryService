using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class FixDepartmentPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_department_positions_positions_PositionId1",
                table: "department_positions");

            migrationBuilder.DropIndex(
                name: "IX_department_positions_PositionId1",
                table: "department_positions");

            migrationBuilder.DropColumn(
                name: "PositionId1",
                table: "department_positions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PositionId1",
                table: "department_positions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_department_positions_PositionId1",
                table: "department_positions",
                column: "PositionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_department_positions_positions_PositionId1",
                table: "department_positions",
                column: "PositionId1",
                principalTable: "positions",
                principalColumn: "id");
        }
    }
}
