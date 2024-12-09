using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataElevatorNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_elevators_building_id",
                schema: "dbo",
                table: "elevators");

            migrationBuilder.AddColumn<int>(
                name: "number",
                schema: "dbo",
                table: "elevators",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("14ef29a8-001e-4b70-93b6-bfdb00237d46"),
                column: "number",
                value: 2);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("82d562f7-f7d5-4088-b735-9a7b085968d3"),
                column: "number",
                value: 6);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("852bb6fa-1831-49ef-a0d9-5bfa5f567841"),
                column: "number",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("966b1041-ff39-432b-917c-b0a14ddce0bd"),
                column: "number",
                value: 3);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("b8557436-6472-4ad7-b111-09c8a023c463"),
                column: "number",
                value: 4);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("bbfbdffa-f7cd-4241-a222-85a733098782"),
                column: "number",
                value: 5);

            migrationBuilder.CreateIndex(
                name: "ix_elevators_building_id_number",
                schema: "dbo",
                table: "elevators",
                columns: new[] { "building_id", "number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_elevators_building_id_number",
                schema: "dbo",
                table: "elevators");

            migrationBuilder.DropColumn(
                name: "number",
                schema: "dbo",
                table: "elevators");

            migrationBuilder.CreateIndex(
                name: "ix_elevators_building_id",
                schema: "dbo",
                table: "elevators",
                column: "building_id");
        }
    }
}
