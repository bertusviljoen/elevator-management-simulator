using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class elevatorstatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("b8557436-6472-4ad7-b111-09c8a023c463"),
                column: "elevator_status",
                value: "Maintenance");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("bbfbdffa-f7cd-4241-a222-85a733098782"),
                column: "elevator_status",
                value: "OutOfService");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("b8557436-6472-4ad7-b111-09c8a023c463"),
                column: "elevator_status",
                value: "Active");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("bbfbdffa-f7cd-4241-a222-85a733098782"),
                column: "elevator_status",
                value: "Active");
        }
    }
}
