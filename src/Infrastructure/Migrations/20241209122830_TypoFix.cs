using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TypoFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("82d562f7-f7d5-4088-b735-9a7b085968d3"));

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("bbfbdffa-f7cd-4241-a222-85a733098782"),
                column: "elevator_type",
                value: "Passenger");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"),
                column: "email",
                value: "admin@building.com");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("bbfbdffa-f7cd-4241-a222-85a733098782"),
                column: "elevator_type",
                value: "Service");

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "elevators",
                columns: new[] { "id", "building_id", "capacity", "created_by_user_id", "created_date_time_utc", "current_floor", "elevator_direction", "elevator_status", "elevator_type", "number", "speed", "updated_by_user_id", "updated_date_time_utc" },
                values: new object[] { new Guid("82d562f7-f7d5-4088-b735-9a7b085968d3"), new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"), 5, new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "None", "Active", "HighSpeed", 6, 1.0, null, null });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"),
                column: "email",
                value: "admin@buiding.com");
        }
    }
}
