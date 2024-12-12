using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "users",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    email = table.Column<string>(type: "TEXT", nullable: false),
                    first_name = table.Column<string>(type: "TEXT", nullable: false),
                    last_name = table.Column<string>(type: "TEXT", nullable: false),
                    password_hash = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "buildings",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    number_of_floors = table.Column<int>(type: "INTEGER", nullable: false),
                    is_default = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_date_time_utc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_date_time_utc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_buildings", x => x.id);
                    table.ForeignKey(
                        name: "fk_buildings_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalSchema: "dbo",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_buildings_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalSchema: "dbo",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "elevators",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    number = table.Column<int>(type: "INTEGER", nullable: false),
                    current_floor = table.Column<int>(type: "INTEGER", nullable: false),
                    destination_floor = table.Column<int>(type: "INTEGER", nullable: false),
                    destination_floors = table.Column<string>(type: "TEXT", nullable: false),
                    door_status = table.Column<int>(type: "INTEGER", nullable: false),
                    elevator_direction = table.Column<string>(type: "TEXT", nullable: false),
                    elevator_status = table.Column<string>(type: "TEXT", nullable: false),
                    elevator_type = table.Column<string>(type: "TEXT", nullable: false),
                    floors_per_second = table.Column<int>(type: "INTEGER", nullable: false),
                    queue_capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    building_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created_date_time_utc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_date_time_utc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_elevators", x => x.id);
                    table.ForeignKey(
                        name: "fk_elevators_buildings_building_id",
                        column: x => x.building_id,
                        principalSchema: "dbo",
                        principalTable: "buildings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_elevators_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalSchema: "dbo",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_elevators_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalSchema: "dbo",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "users",
                columns: new[] { "id", "email", "first_name", "last_name", "password_hash" },
                values: new object[] { new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), "admin@building.com", "Admin", "Joe", "55BC042899399B562DD4A363FD250A9014C045B900716FCDC074861EB69C344A-B44367BE2D0B037E31AEEE2649199100" });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "buildings",
                columns: new[] { "id", "created_by_user_id", "created_date_time_utc", "is_default", "name", "number_of_floors", "updated_by_user_id", "updated_date_time_utc" },
                values: new object[] { new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"), new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Joe's Building", 10, null, null });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "elevators",
                columns: new[] { "id", "building_id", "created_by_user_id", "created_date_time_utc", "current_floor", "destination_floor", "destination_floors", "door_status", "elevator_direction", "elevator_status", "elevator_type", "floors_per_second", "number", "queue_capacity", "updated_by_user_id", "updated_date_time_utc" },
                values: new object[,]
                {
                    { new Guid("14ef29a8-001e-4b70-93b6-bfdb00237d46"), new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"), new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 0, "", 0, "None", "Active", "Passenger", 1, 2, 3, null, null },
                    { new Guid("852bb6fa-1831-49ef-a0d9-5bfa5f567841"), new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"), new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 0, "", 0, "None", "Active", "HighSpeed", 5, 1, 3, null, null },
                    { new Guid("966b1041-ff39-432b-917c-b0a14ddce0bd"), new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"), new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 0, "", 0, "None", "Active", "Passenger", 1, 3, 3, null, null },
                    { new Guid("b8557436-6472-4ad7-b111-09c8a023c463"), new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"), new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 0, "", 0, "None", "Maintenance", "Passenger", 1, 4, 3, null, null },
                    { new Guid("bbfbdffa-f7cd-4241-a222-85a733098782"), new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"), new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 0, "", 0, "None", "OutOfService", "Passenger", 1, 5, 3, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_buildings_created_by_user_id",
                schema: "dbo",
                table: "buildings",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_buildings_name",
                schema: "dbo",
                table: "buildings",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_buildings_updated_by_user_id",
                schema: "dbo",
                table: "buildings",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_elevators_building_id_number",
                schema: "dbo",
                table: "elevators",
                columns: new[] { "building_id", "number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_elevators_created_by_user_id",
                schema: "dbo",
                table: "elevators",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_elevators_updated_by_user_id",
                schema: "dbo",
                table: "elevators",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "dbo",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "elevators",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "buildings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "users",
                schema: "dbo");
        }
    }
}
