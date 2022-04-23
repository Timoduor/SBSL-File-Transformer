using Microsoft.EntityFrameworkCore.Migrations;

namespace SbslFileTransformer.Migrations
{
    public partial class extralogging : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MatchingFile",
                table: "VisionRecords",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchingFile",
                table: "VisionRecordDebtors",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchingFile",
                table: "VisionRecordCreditSett",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchingFile",
                table: "VisionRecords");

            migrationBuilder.DropColumn(
                name: "MatchingFile",
                table: "VisionRecordDebtors");

            migrationBuilder.DropColumn(
                name: "MatchingFile",
                table: "VisionRecordCreditSett");
        }
    }
}
