using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addbang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBaoDuong",
                table: "LichSuBaoDuongs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDuyet",
                table: "LichSuBaoDuongs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHoanThanh",
                table: "LichSuBaoDuongs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsYeuCau",
                table: "LichSuBaoDuongs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBaoDuong",
                table: "LichSuBaoDuongs");

            migrationBuilder.DropColumn(
                name: "IsDuyet",
                table: "LichSuBaoDuongs");

            migrationBuilder.DropColumn(
                name: "IsHoanThanh",
                table: "LichSuBaoDuongs");

            migrationBuilder.DropColumn(
                name: "IsYeuCau",
                table: "LichSuBaoDuongs");
        }
    }
}
