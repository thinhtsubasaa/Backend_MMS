using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addlsnew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "DS_ThietBis");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "DS_PhuongTiens");

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "LichSuBaoDuongs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "LichSuBaoDuongs");

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "DS_ThietBis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "DS_PhuongTiens",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
