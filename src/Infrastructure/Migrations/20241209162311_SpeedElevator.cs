using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SpeedElevator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "floors_per_second",
                schema: "dbo",
                table: "elevators",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "REAL");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("14ef29a8-001e-4b70-93b6-bfdb00237d46"),
                column: "floors_per_second",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("852bb6fa-1831-49ef-a0d9-5bfa5f567841"),
                columns: new[] { "elevator_type", "floors_per_second" },
                values: new object[] { "HighSpeed", 5 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("966b1041-ff39-432b-917c-b0a14ddce0bd"),
                column: "floors_per_second",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("b8557436-6472-4ad7-b111-09c8a023c463"),
                column: "floors_per_second",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("bbfbdffa-f7cd-4241-a222-85a733098782"),
                column: "floors_per_second",
                value: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "floors_per_second",
                schema: "dbo",
                table: "elevators",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("14ef29a8-001e-4b70-93b6-bfdb00237d46"),
                column: "floors_per_second",
                value: 1.0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("852bb6fa-1831-49ef-a0d9-5bfa5f567841"),
                columns: new[] { "elevator_type", "floors_per_second" },
                values: new object[] { "Passenger", 1.0 });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("966b1041-ff39-432b-917c-b0a14ddce0bd"),
                column: "floors_per_second",
                value: 1.0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("b8557436-6472-4ad7-b111-09c8a023c463"),
                column: "floors_per_second",
                value: 1.0);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("bbfbdffa-f7cd-4241-a222-85a733098782"),
                column: "floors_per_second",
                value: 1.0);
        }
    }
}
