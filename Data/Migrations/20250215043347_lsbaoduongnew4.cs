using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class lsbaoduongnew4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "NguoiXacNhan_Id",
                table: "LichSuBaoDuongs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NguoiYeuCau_Id",
                table: "LichSuBaoDuongs",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NguoiXacNhan_Id",
                table: "LichSuBaoDuongs");

            migrationBuilder.DropColumn(
                name: "NguoiYeuCau_Id",
                table: "LichSuBaoDuongs");
        }
    }
}
