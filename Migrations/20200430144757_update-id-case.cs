using Microsoft.EntityFrameworkCore.Migrations;

namespace temp_tracker.Migrations
{
    public partial class updateidcase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReadingID",
                table: "Readings",
                newName: "ReadingId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReadingId",
                table: "Readings",
                newName: "ReadingID");
        }
    }
}
