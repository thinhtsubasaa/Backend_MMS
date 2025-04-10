using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addthietbinew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LichSuBaoDuong_Id",
                table: "DS_ThietBis",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TinhTrang_Id",
                table: "DS_ThietBis",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DS_ThietBis_LichSuBaoDuong_Id",
                table: "DS_ThietBis",
                column: "LichSuBaoDuong_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DS_ThietBis_TinhTrang_Id",
                table: "DS_ThietBis",
                column: "TinhTrang_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DS_ThietBis_DM_TinhTrangs_TinhTrang_Id",
                table: "DS_ThietBis",
                column: "TinhTrang_Id",
                principalTable: "DM_TinhTrangs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DS_ThietBis_LichSuBaoDuongs_LichSuBaoDuong_Id",
                table: "DS_ThietBis",
                column: "LichSuBaoDuong_Id",
                principalTable: "LichSuBaoDuongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DS_ThietBis_DM_TinhTrangs_TinhTrang_Id",
                table: "DS_ThietBis");

            migrationBuilder.DropForeignKey(
                name: "FK_DS_ThietBis_LichSuBaoDuongs_LichSuBaoDuong_Id",
                table: "DS_ThietBis");

            migrationBuilder.DropIndex(
                name: "IX_DS_ThietBis_LichSuBaoDuong_Id",
                table: "DS_ThietBis");

            migrationBuilder.DropIndex(
                name: "IX_DS_ThietBis_TinhTrang_Id",
                table: "DS_ThietBis");

            migrationBuilder.DropColumn(
                name: "LichSuBaoDuong_Id",
                table: "DS_ThietBis");

            migrationBuilder.DropColumn(
                name: "TinhTrang_Id",
                table: "DS_ThietBis");
        }
    }
}
