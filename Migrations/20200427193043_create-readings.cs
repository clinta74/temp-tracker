using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace temp_tracker.Migrations
{
    public partial class createreadings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Readings",
                columns: table => new
                {
                    ReadingID = table.Column<Guid>(nullable: false),
                    Value = table.Column<decimal>(nullable: false),
                    Scale = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readings", x => x.ReadingID);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Readings");
        }
    }
}
