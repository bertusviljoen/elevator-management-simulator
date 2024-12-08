using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "elevator",
                schema: "dbo",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    current_floor = table.Column<int>(type: "INTEGER", nullable: false),
                    elevator_direction = table.Column<string>(type: "TEXT", nullable: false),
                    elevator_status = table.Column<string>(type: "TEXT", nullable: false),
                    elevator_type = table.Column<string>(type: "TEXT", nullable: false),
                    speed = table.Column<double>(type: "REAL", nullable: false),
                    capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    building_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    created_date_time_utc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_date_time_utc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_by_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    updated_by_user_id = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_elevator", x => x.id);
                    table.ForeignKey(
                        name: "fk_elevator_buildings_building_id",
                        column: x => x.building_id,
                        principalSchema: "dbo",
                        principalTable: "buildings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_elevator_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalSchema: "dbo",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_elevator_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalSchema: "dbo",
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "users",
                columns: new[] { "id", "email", "first_name", "last_name", "password_hash" },
                values: new object[] { new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), "admin@buiding.com", "Admin", "Joe", "530CDAF49C27B049D32949948AC28BB6DAAD98B1C48C971284B32DC83B1DB196-F03C1CB1D5D328F210451652459070FA" });

            migrationBuilder.CreateIndex(
                name: "ix_elevator_building_id",
                schema: "dbo",
                table: "elevator",
                column: "building_id");

            migrationBuilder.CreateIndex(
                name: "ix_elevator_created_by_user_id",
                schema: "dbo",
                table: "elevator",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_elevator_updated_by_user_id",
                schema: "dbo",
                table: "elevator",
                column: "updated_by_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "elevator",
                schema: "dbo");

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"));
        }
    }
}
