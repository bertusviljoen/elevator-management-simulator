using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ElevatorBuilding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "speed",
                schema: "dbo",
                table: "elevators",
                newName: "floors_per_second");

            migrationBuilder.RenameColumn(
                name: "capacity",
                schema: "dbo",
                table: "elevators",
                newName: "person_capacity");

            migrationBuilder.AddColumn<int>(
                name: "destination_floor",
                schema: "dbo",
                table: "elevators",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "destination_floors",
                schema: "dbo",
                table: "elevators",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "door_status",
                schema: "dbo",
                table: "elevators",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_default",
                schema: "dbo",
                table: "buildings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "buildings",
                keyColumn: "id",
                keyValue: new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"),
                column: "is_default",
                value: true);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("14ef29a8-001e-4b70-93b6-bfdb00237d46"),
                columns: new[] { "destination_floor", "destination_floors", "door_status", "floors_per_second" },
                values: new object[] { 0, "", 0, 1.0 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("852bb6fa-1831-49ef-a0d9-5bfa5f567841"),
                columns: new[] { "destination_floor", "destination_floors", "door_status", "floors_per_second" },
                values: new object[] { 0, "", 0, 1.0 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("966b1041-ff39-432b-917c-b0a14ddce0bd"),
                columns: new[] { "destination_floor", "destination_floors", "door_status", "floors_per_second" },
                values: new object[] { 0, "", 0, 1.0 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("b8557436-6472-4ad7-b111-09c8a023c463"),
                columns: new[] { "destination_floor", "destination_floors", "door_status", "floors_per_second" },
                values: new object[] { 0, "", 0, 1.0 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("bbfbdffa-f7cd-4241-a222-85a733098782"),
                columns: new[] { "destination_floor", "destination_floors", "door_status", "floors_per_second" },
                values: new object[] { 0, "", 0, 1.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "destination_floor",
                schema: "dbo",
                table: "elevators");

            migrationBuilder.DropColumn(
                name: "destination_floors",
                schema: "dbo",
                table: "elevators");

            migrationBuilder.DropColumn(
                name: "door_status",
                schema: "dbo",
                table: "elevators");

            migrationBuilder.DropColumn(
                name: "is_default",
                schema: "dbo",
                table: "buildings");

            migrationBuilder.RenameColumn(
                name: "person_capacity",
                schema: "dbo",
                table: "elevators",
                newName: "capacity");

            migrationBuilder.RenameColumn(
                name: "floors_per_second",
                schema: "dbo",
                table: "elevators",
                newName: "speed");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("14ef29a8-001e-4b70-93b6-bfdb00237d46"),
                column: "speed",
                value: 0.5);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("852bb6fa-1831-49ef-a0d9-5bfa5f567841"),
                column: "speed",
                value: 0.5);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("966b1041-ff39-432b-917c-b0a14ddce0bd"),
                column: "speed",
                value: 0.5);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("b8557436-6472-4ad7-b111-09c8a023c463"),
                column: "speed",
                value: 0.5);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("bbfbdffa-f7cd-4241-a222-85a733098782"),
                column: "speed",
                value: 0.5);
        }
    }
}
