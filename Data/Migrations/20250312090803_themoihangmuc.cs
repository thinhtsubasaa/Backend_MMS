using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class themoihangmuc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CanhBao_DenHan",
                table: "DM_HangMucs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CanhBao_GanDenHan",
                table: "DM_HangMucs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "LoaiPT_Id",
                table: "DM_HangMucs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DM_HangMucs_LoaiPT_Id",
                table: "DM_HangMucs",
                column: "LoaiPT_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DM_HangMucs_DM_Loais_LoaiPT_Id",
                table: "DM_HangMucs",
                column: "LoaiPT_Id",
                principalTable: "DM_Loais",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DM_HangMucs_DM_Loais_LoaiPT_Id",
                table: "DM_HangMucs");

            migrationBuilder.DropIndex(
                name: "IX_DM_HangMucs_LoaiPT_Id",
                table: "DM_HangMucs");

            migrationBuilder.DropColumn(
                name: "CanhBao_DenHan",
                table: "DM_HangMucs");

            migrationBuilder.DropColumn(
                name: "CanhBao_GanDenHan",
                table: "DM_HangMucs");

            migrationBuilder.DropColumn(
                name: "LoaiPT_Id",
                table: "DM_HangMucs");
        }
    }
}
