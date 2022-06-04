using Microsoft.EntityFrameworkCore.Migrations;

namespace SbslFileTransformer.Migrations
{
    public partial class addedfinacleaccountcol : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FinacleAccount",
                table: "VisionRecords",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinacleAccount",
                table: "VisionRecordDebtors",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinacleAccount",
                table: "VisionRecordCreditSett",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinacleAccount",
                table: "VisionRecords");

            migrationBuilder.DropColumn(
                name: "FinacleAccount",
                table: "VisionRecordDebtors");

            migrationBuilder.DropColumn(
                name: "FinacleAccount",
                table: "VisionRecordCreditSett");
        }
    }
}
