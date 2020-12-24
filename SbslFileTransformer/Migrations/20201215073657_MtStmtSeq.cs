using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace SbslFileTransformer.Migrations
{
    public partial class MtStmtSeq : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "UploadedDate",
            //    table: "UploadedFiles",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "TEXT",
            //    oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MtSequenceNo",
                table: "UploadedFiles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MtStatementNo",
                table: "UploadedFiles",
                nullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "Value",
            //    table: "Configurations",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "TEXT",
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "Key",
            //    table: "Configurations",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "TEXT",
            //    oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MtSequenceNo",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "MtStatementNo",
                table: "UploadedFiles");

            migrationBuilder.AlterColumn<string>(
                name: "UploadedDate",
                table: "UploadedFiles",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Configurations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "Configurations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
