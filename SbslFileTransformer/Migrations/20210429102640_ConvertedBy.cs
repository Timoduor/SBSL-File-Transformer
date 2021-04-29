using Microsoft.EntityFrameworkCore.Migrations;

namespace SbslFileTransformer.Migrations
{
    public partial class ConvertedBy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConvertedBy",
                table: "UploadedFiles",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Failed",
                table: "UploadedFiles",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConvertedBy",
                table: "UploadedFiles");

            migrationBuilder.DropColumn(
                name: "Failed",
                table: "UploadedFiles");
        }
    }
}
