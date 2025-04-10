using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addbang2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayDiBaoDuong",
                table: "LichSuBaoDuongs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayHoanThanh",
                table: "LichSuBaoDuongs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "NguoiDiBaoDuong",
                table: "LichSuBaoDuongs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NguoiDiBaoDuong_Id",
                table: "LichSuBaoDuongs",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayDiBaoDuong",
                table: "LichSuBaoDuongs");

            migrationBuilder.DropColumn(
                name: "NgayHoanThanh",
                table: "LichSuBaoDuongs");

            migrationBuilder.DropColumn(
                name: "NguoiDiBaoDuong",
                table: "LichSuBaoDuongs");

            migrationBuilder.DropColumn(
                name: "NguoiDiBaoDuong_Id",
                table: "LichSuBaoDuongs");
        }
    }
}
