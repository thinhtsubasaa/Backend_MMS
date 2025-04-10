using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addbang6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LichSuBaoDuong_Id",
                table: "DS_PhuongTiens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DS_PhuongTiens_LichSuBaoDuong_Id",
                table: "DS_PhuongTiens",
                column: "LichSuBaoDuong_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DS_PhuongTiens_LichSuBaoDuongs_LichSuBaoDuong_Id",
                table: "DS_PhuongTiens",
                column: "LichSuBaoDuong_Id",
                principalTable: "LichSuBaoDuongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DS_PhuongTiens_LichSuBaoDuongs_LichSuBaoDuong_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.DropIndex(
                name: "IX_DS_PhuongTiens_LichSuBaoDuong_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.DropColumn(
                name: "LichSuBaoDuong_Id",
                table: "DS_PhuongTiens");
        }
    }
}
