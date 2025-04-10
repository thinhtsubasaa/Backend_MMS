using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addbang3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NguoiXacNhanHoanThanh",
                table: "LichSuBaoDuongs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NguoiXacNhanHoanThanh_Id",
                table: "LichSuBaoDuongs",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NguoiXacNhanHoanThanh",
                table: "LichSuBaoDuongs");

            migrationBuilder.DropColumn(
                name: "NguoiXacNhanHoanThanh_Id",
                table: "LichSuBaoDuongs");
        }
    }
}
