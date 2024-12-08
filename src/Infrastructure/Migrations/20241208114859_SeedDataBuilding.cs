using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataBuilding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_elevator_buildings_building_id",
                schema: "dbo",
                table: "elevator");

            migrationBuilder.DropForeignKey(
                name: "fk_elevator_users_created_by_user_id",
                schema: "dbo",
                table: "elevator");

            migrationBuilder.DropForeignKey(
                name: "fk_elevator_users_updated_by_user_id",
                schema: "dbo",
                table: "elevator");

            migrationBuilder.DropPrimaryKey(
                name: "pk_elevator",
                schema: "dbo",
                table: "elevator");

            migrationBuilder.RenameTable(
                name: "elevator",
                schema: "dbo",
                newName: "elevators",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "ix_elevator_updated_by_user_id",
                schema: "dbo",
                table: "elevators",
                newName: "ix_elevators_updated_by_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_elevator_created_by_user_id",
                schema: "dbo",
                table: "elevators",
                newName: "ix_elevators_created_by_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_elevator_building_id",
                schema: "dbo",
                table: "elevators",
                newName: "ix_elevators_building_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_elevators",
                schema: "dbo",
                table: "elevators",
                column: "id");

            migrationBuilder.InsertData(
                schema: "dbo",
                table: "buildings",
                columns: new[] { "id", "created_by_user_id", "created_date_time_utc", "name", "number_of_floors", "updated_by_user_id", "updated_date_time_utc" },
                values: new object[] { new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"), new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Joe's Building", 10, null, null });

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"),
                column: "password_hash",
                value: "55BC042899399B562DD4A363FD250A9014C045B900716FCDC074861EB69C344A-B44367BE2D0B037E31AEEE2649199100");

            migrationBuilder.AddForeignKey(
                name: "fk_elevators_buildings_building_id",
                schema: "dbo",
                table: "elevators",
                column: "building_id",
                principalSchema: "dbo",
                principalTable: "buildings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_elevators_users_created_by_user_id",
                schema: "dbo",
                table: "elevators",
                column: "created_by_user_id",
                principalSchema: "dbo",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_elevators_users_updated_by_user_id",
                schema: "dbo",
                table: "elevators",
                column: "updated_by_user_id",
                principalSchema: "dbo",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_elevators_buildings_building_id",
                schema: "dbo",
                table: "elevators");

            migrationBuilder.DropForeignKey(
                name: "fk_elevators_users_created_by_user_id",
                schema: "dbo",
                table: "elevators");

            migrationBuilder.DropForeignKey(
                name: "fk_elevators_users_updated_by_user_id",
                schema: "dbo",
                table: "elevators");

            migrationBuilder.DropPrimaryKey(
                name: "pk_elevators",
                schema: "dbo",
                table: "elevators");

            migrationBuilder.DeleteData(
                schema: "dbo",
                table: "buildings",
                keyColumn: "id",
                keyValue: new Guid("e16e32e7-8db0-4536-b86e-f53e53cd7a0d"));

            migrationBuilder.RenameTable(
                name: "elevators",
                schema: "dbo",
                newName: "elevator",
                newSchema: "dbo");

            migrationBuilder.RenameIndex(
                name: "ix_elevators_updated_by_user_id",
                schema: "dbo",
                table: "elevator",
                newName: "ix_elevator_updated_by_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_elevators_created_by_user_id",
                schema: "dbo",
                table: "elevator",
                newName: "ix_elevator_created_by_user_id");

            migrationBuilder.RenameIndex(
                name: "ix_elevators_building_id",
                schema: "dbo",
                table: "elevator",
                newName: "ix_elevator_building_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_elevator",
                schema: "dbo",
                table: "elevator",
                column: "id");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("31a9cff7-dc59-4135-a762-6e814bab6f9a"),
                column: "password_hash",
                value: "530CDAF49C27B049D32949948AC28BB6DAAD98B1C48C971284B32DC83B1DB196-F03C1CB1D5D328F210451652459070FA");

            migrationBuilder.AddForeignKey(
                name: "fk_elevator_buildings_building_id",
                schema: "dbo",
                table: "elevator",
                column: "building_id",
                principalSchema: "dbo",
                principalTable: "buildings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_elevator_users_created_by_user_id",
                schema: "dbo",
                table: "elevator",
                column: "created_by_user_id",
                principalSchema: "dbo",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_elevator_users_updated_by_user_id",
                schema: "dbo",
                table: "elevator",
                column: "updated_by_user_id",
                principalSchema: "dbo",
                principalTable: "users",
                principalColumn: "id");
        }
    }
}
