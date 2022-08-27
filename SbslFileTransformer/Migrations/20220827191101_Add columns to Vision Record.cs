using Microsoft.EntityFrameworkCore.Migrations;

namespace SbslFileTransformer.Migrations
{
    public partial class AddcolumnstoVisionRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorizationCode",
                table: "VisionRecords",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChequeNo",
                table: "VisionRecords",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorizationCode",
                table: "VisionRecordDebtors",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChequeNo",
                table: "VisionRecordDebtors",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorizationCode",
                table: "VisionRecordCreditSett",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChequeNo",
                table: "VisionRecordCreditSett",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorizationCode",
                table: "VisionRecords");

            migrationBuilder.DropColumn(
                name: "ChequeNo",
                table: "VisionRecords");

            migrationBuilder.DropColumn(
                name: "AuthorizationCode",
                table: "VisionRecordDebtors");

            migrationBuilder.DropColumn(
                name: "ChequeNo",
                table: "VisionRecordDebtors");

            migrationBuilder.DropColumn(
                name: "AuthorizationCode",
                table: "VisionRecordCreditSett");

            migrationBuilder.DropColumn(
                name: "ChequeNo",
                table: "VisionRecordCreditSett");
        }
    }
}
