using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addphutrachnew7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NguoiHuyYeuCau_Id",
                table: "LichSuBaoDuongs",
                newName: "NguoiHuyDuyet_Id");

            migrationBuilder.RenameColumn(
                name: "NguoiHuyYeuCau",
                table: "LichSuBaoDuongs",
                newName: "NguoiHuyDuyet");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NguoiHuyDuyet_Id",
                table: "LichSuBaoDuongs",
                newName: "NguoiHuyYeuCau_Id");

            migrationBuilder.RenameColumn(
                name: "NguoiHuyDuyet",
                table: "LichSuBaoDuongs",
                newName: "NguoiHuyYeuCau");
        }
    }
}
