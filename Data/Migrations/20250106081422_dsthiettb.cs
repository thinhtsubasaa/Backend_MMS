using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class dsthiettb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "NgayBatDau",
                table: "DS_ThietBis",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LoaiTB_Id",
                table: "DS_ThietBis",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DonVi_Id",
                table: "DS_PhuongTiens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LoaiPT_Id",
                table: "DS_PhuongTiens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Model_Id",
                table: "DS_PhuongTiens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TinhTrang_Id",
                table: "DS_PhuongTiens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaDV",
                table: "DM_DonVis",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaBP",
                table: "DM_BoPhans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DS_ThietBis_LoaiTB_Id",
                table: "DS_ThietBis",
                column: "LoaiTB_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DS_PhuongTiens_DonVi_Id",
                table: "DS_PhuongTiens",
                column: "DonVi_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DS_PhuongTiens_LoaiPT_Id",
                table: "DS_PhuongTiens",
                column: "LoaiPT_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DS_PhuongTiens_Model_Id",
                table: "DS_PhuongTiens",
                column: "Model_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DS_PhuongTiens_TinhTrang_Id",
                table: "DS_PhuongTiens",
                column: "TinhTrang_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DS_PhuongTiens_DM_DonVis_DonVi_Id",
                table: "DS_PhuongTiens",
                column: "DonVi_Id",
                principalTable: "DM_DonVis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DS_PhuongTiens_DM_Loais_LoaiPT_Id",
                table: "DS_PhuongTiens",
                column: "LoaiPT_Id",
                principalTable: "DM_Loais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DS_PhuongTiens_DM_Models_Model_Id",
                table: "DS_PhuongTiens",
                column: "Model_Id",
                principalTable: "DM_Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DS_PhuongTiens_DM_TinhTrangs_TinhTrang_Id",
                table: "DS_PhuongTiens",
                column: "TinhTrang_Id",
                principalTable: "DM_TinhTrangs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DS_ThietBis_DM_Loais_LoaiTB_Id",
                table: "DS_ThietBis",
                column: "LoaiTB_Id",
                principalTable: "DM_Loais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DS_PhuongTiens_DM_DonVis_DonVi_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.DropForeignKey(
                name: "FK_DS_PhuongTiens_DM_Loais_LoaiPT_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.DropForeignKey(
                name: "FK_DS_PhuongTiens_DM_Models_Model_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.DropForeignKey(
                name: "FK_DS_PhuongTiens_DM_TinhTrangs_TinhTrang_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.DropForeignKey(
                name: "FK_DS_ThietBis_DM_Loais_LoaiTB_Id",
                table: "DS_ThietBis");

            migrationBuilder.DropIndex(
                name: "IX_DS_ThietBis_LoaiTB_Id",
                table: "DS_ThietBis");

            migrationBuilder.DropIndex(
                name: "IX_DS_PhuongTiens_DonVi_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.DropIndex(
                name: "IX_DS_PhuongTiens_LoaiPT_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.DropIndex(
                name: "IX_DS_PhuongTiens_Model_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.DropIndex(
                name: "IX_DS_PhuongTiens_TinhTrang_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.DropColumn(
                name: "LoaiTB_Id",
                table: "DS_ThietBis");

            migrationBuilder.DropColumn(
                name: "DonVi_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.DropColumn(
                name: "LoaiPT_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.DropColumn(
                name: "Model_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.DropColumn(
                name: "TinhTrang_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.DropColumn(
                name: "MaDV",
                table: "DM_DonVis");

            migrationBuilder.DropColumn(
                name: "MaBP",
                table: "DM_BoPhans");

            migrationBuilder.AlterColumn<string>(
                name: "NgayBatDau",
                table: "DS_ThietBis",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }
    }
}
