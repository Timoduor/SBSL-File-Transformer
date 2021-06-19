using Microsoft.EntityFrameworkCore.Migrations;

namespace SbslFileTransformer.Migrations
{
    public partial class Reporting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Country",
                table: "EmailGroups",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Sprint",
                table: "EmailGroups",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Country",
                table: "EmailGroups");

            migrationBuilder.DropColumn(
                name: "Sprint",
                table: "EmailGroups");
        }
    }
}
