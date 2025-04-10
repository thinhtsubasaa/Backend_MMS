using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addpt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SoChuyenXe",
                table: "DS_PhuongTiens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoKM",
                table: "DS_PhuongTiens",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoKhung",
                table: "DS_PhuongTiens",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoChuyenXe",
                table: "DS_PhuongTiens");

            migrationBuilder.DropColumn(
                name: "SoKM",
                table: "DS_PhuongTiens");

            migrationBuilder.DropColumn(
                name: "SoKhung",
                table: "DS_PhuongTiens");
        }
    }
}
