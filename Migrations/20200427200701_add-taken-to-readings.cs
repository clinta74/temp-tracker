using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace temp_tracker.Migrations
{
    public partial class addtakentoreadings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Taken",
                table: "Readings",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Taken",
                table: "Readings");
        }
    }
}
