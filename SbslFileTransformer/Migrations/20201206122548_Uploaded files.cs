using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace SbslFileTransformer.Migrations
{
    public partial class Uploadedfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UploadedFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Md5 = table.Column<string>(nullable: true),
                    UploadedDate = table.Column<string>(nullable: true),
                    Size = table.Column<long>(nullable: false),
                    IsProduction = table.Column<bool>(nullable: false),
                    FilePath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFiles", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UploadedFiles");
        }
    }
}
