using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addbophan7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PhuongTien_Id",
                table: "LichSuPhanBoDonVis",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LichSuPhanBoDonVis_PhuongTien_Id",
                table: "LichSuPhanBoDonVis",
                column: "PhuongTien_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuPhanBoDonVis_DS_PhuongTiens_PhuongTien_Id",
                table: "LichSuPhanBoDonVis",
                column: "PhuongTien_Id",
                principalTable: "DS_PhuongTiens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichSuPhanBoDonVis_DS_PhuongTiens_PhuongTien_Id",
                table: "LichSuPhanBoDonVis");

            migrationBuilder.DropIndex(
                name: "IX_LichSuPhanBoDonVis_PhuongTien_Id",
                table: "LichSuPhanBoDonVis");

            migrationBuilder.DropColumn(
                name: "PhuongTien_Id",
                table: "LichSuPhanBoDonVis");
        }
    }
}
