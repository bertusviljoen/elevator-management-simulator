using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataElevators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "dbo",
                table: "elevators",
                columns: new[] { "id", "building_id", "capacity", "created_by_user_id", "created_date_time_utc", "current_floor", "elevator_direction", "elevator_status", "elevator_type", "speed", "updated_by_user_id", "updated_date_time_utc" },
                values: new object[,]
                {
                    { new Guid("14ef29a8-001e-4b70-93b6-bfdb00237d46"), new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"), 10, new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "None", "Active", "Passenger", 0.5, null, null },
                    { new Guid("82d562f7-f7d5-4088-b735-9a7b085968d3"), new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"), 5, new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "None", "Active", "HighSpeed", 1.0, null, null },
                    { new Guid("852bb6fa-1831-49ef-a0d9-5bfa5f567841"), new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"), 10, new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "None", "Active", "Passenger", 0.5, null, null },
                    { new Guid("966b1041-ff39-432b-917c-b0a14ddce0bd"), new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"), 10, new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "None", "Active", "Passenger", 0.5, null, null },
                    { new Guid("b8557436-6472-4ad7-b111-09c8a023c463"), new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"), 10, new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "None", "Active", "Passenger", 0.5, null, null },
                    { new Guid("bbfbdffa-f7cd-4241-a222-85a733098782"), new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"), 10, new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "None", "Active", "Service", 0.5, null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("14ef29a8-001e-4b70-93b6-bfdb00237d46"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("82d562f7-f7d5-4088-b735-9a7b085968d3"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("852bb6fa-1831-49ef-a0d9-5bfa5f567841"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("966b1041-ff39-432b-917c-b0a14ddce0bd"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("b8557436-6472-4ad7-b111-09c8a023c463"));

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("bbfbdffa-f7cd-4241-a222-85a733098782"));
        }
    }
}
