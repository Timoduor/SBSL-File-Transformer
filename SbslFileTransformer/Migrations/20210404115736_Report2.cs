using Microsoft.EntityFrameworkCore.Migrations;

namespace SbslFileTransformer.Migrations
{
    public partial class Report2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Creator",
                table: "ProcessedReports",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndTime",
                table: "ProcessedReports",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "ProcessedReports",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ProcessedReports",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartTime",
                table: "ProcessedReports",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ProcessedReports",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserToken",
                table: "ProcessedReports",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Creator",
                table: "ProcessedReports");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ProcessedReports");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "ProcessedReports");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ProcessedReports");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ProcessedReports");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ProcessedReports");

            migrationBuilder.DropColumn(
                name: "UserToken",
                table: "ProcessedReports");
        }
    }
}
