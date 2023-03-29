using Microsoft.EntityFrameworkCore.Migrations;

namespace SbslFileTransformer.Migrations
{
    public partial class ExtracolumnforPrimEntIDT : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrimaryEntryIDT",
                table: "VisionRecords",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryEntryIDT",
                table: "VisionRecordDebtors",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryEntryIDT",
                table: "VisionRecordCreditSett",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryEntryIDT",
                table: "VisionRecords");

            migrationBuilder.DropColumn(
                name: "PrimaryEntryIDT",
                table: "VisionRecordDebtors");

            migrationBuilder.DropColumn(
                name: "PrimaryEntryIDT",
                table: "VisionRecordCreditSett");
        }
    }
}
