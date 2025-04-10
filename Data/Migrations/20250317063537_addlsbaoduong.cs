using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addlsbaoduong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayDeXuatHoanThanh",
                table: "LichSuBaoDuongs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NguoiDeXuatHoanThanh",
                table: "LichSuBaoDuongs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NguoiDeXuatHoanThanh_Id",
                table: "LichSuBaoDuongs",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayDeXuatHoanThanh",
                table: "LichSuBaoDuongs");

            migrationBuilder.DropColumn(
                name: "NguoiDeXuatHoanThanh",
                table: "LichSuBaoDuongs");

            migrationBuilder.DropColumn(
                name: "NguoiDeXuatHoanThanh_Id",
                table: "LichSuBaoDuongs");
        }
    }
}
