using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class qcapasity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "person_capacity",
                schema: "dbo",
                table: "elevators",
                newName: "queue_capacity");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("14ef29a8-001e-4b70-93b6-bfdb00237d46"),
                column: "queue_capacity",
                value: 3);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("852bb6fa-1831-49ef-a0d9-5bfa5f567841"),
                column: "queue_capacity",
                value: 3);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("966b1041-ff39-432b-917c-b0a14ddce0bd"),
                column: "queue_capacity",
                value: 3);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("b8557436-6472-4ad7-b111-09c8a023c463"),
                column: "queue_capacity",
                value: 3);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("bbfbdffa-f7cd-4241-a222-85a733098782"),
                column: "queue_capacity",
                value: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "queue_capacity",
                schema: "dbo",
                table: "elevators",
                newName: "person_capacity");

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("14ef29a8-001e-4b70-93b6-bfdb00237d46"),
                column: "person_capacity",
                value: 10);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("852bb6fa-1831-49ef-a0d9-5bfa5f567841"),
                column: "person_capacity",
                value: 10);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("966b1041-ff39-432b-917c-b0a14ddce0bd"),
                column: "person_capacity",
                value: 10);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("b8557436-6472-4ad7-b111-09c8a023c463"),
                column: "person_capacity",
                value: 10);

            migrationBuilder.UpdateData(
                schema: "dbo",
                table: "elevators",
                keyColumn: "id",
                keyValue: new Guid("bbfbdffa-f7cd-4241-a222-85a733098782"),
                column: "person_capacity",
                value: 10);
        }
    }
}
