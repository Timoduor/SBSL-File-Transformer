using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SbslFileTransformer.Migrations
{
    public partial class Updatevisionrecordsstructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VisionRecordCreditSett",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    BankingDate = table.Column<DateTime>(nullable: false),
                    TransDetails = table.Column<string>(nullable: true),
                    TransID = table.Column<string>(nullable: true),
                    ReferenceNumber = table.Column<string>(nullable: true),
                    GLTransCode = table.Column<string>(nullable: true),
                    CardNumber = table.Column<string>(nullable: true),
                    CreditAmount = table.Column<double>(nullable: false),
                    DebitAmount = table.Column<double>(nullable: false),
                    CustomerName = table.Column<string>(nullable: true),
                    ContractNumber = table.Column<string>(nullable: true),
                    AccountNumber = table.Column<string>(nullable: true),
                    Matched = table.Column<bool>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    DateExtracted = table.Column<DateTime>(nullable: false),
                    DateMatched = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisionRecordCreditSett", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisionRecordDebtors",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    BankingDate = table.Column<DateTime>(nullable: false),
                    TransDetails = table.Column<string>(nullable: true),
                    TransID = table.Column<string>(nullable: true),
                    ReferenceNumber = table.Column<string>(nullable: true),
                    GLTransCode = table.Column<string>(nullable: true),
                    CardNumber = table.Column<string>(nullable: true),
                    CreditAmount = table.Column<double>(nullable: false),
                    DebitAmount = table.Column<double>(nullable: false),
                    CustomerName = table.Column<string>(nullable: true),
                    ContractNumber = table.Column<string>(nullable: true),
                    AccountNumber = table.Column<string>(nullable: true),
                    Matched = table.Column<bool>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    DateExtracted = table.Column<DateTime>(nullable: false),
                    DateMatched = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisionRecordDebtors", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VisionRecordCreditSett");

            migrationBuilder.DropTable(
                name: "VisionRecordDebtors");
        }
    }
}
