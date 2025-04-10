using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class lsbaoduongnew2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NguoiXacNhan",
                table: "LichSuBaoDuongs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiYeuCau",
                table: "LichSuBaoDuongs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NguoiXacNhan",
                table: "LichSuBaoDuongs");

            migrationBuilder.DropColumn(
                name: "NguoiYeuCau",
                table: "LichSuBaoDuongs");
        }
    }
}
