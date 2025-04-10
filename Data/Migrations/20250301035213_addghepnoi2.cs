using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addghepnoi2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GhepNoiPhuongTien_ThietBis_DS_ThietBis_ThietBi2Id",
                table: "GhepNoiPhuongTien_ThietBis");

            migrationBuilder.DropForeignKey(
                name: "FK_GhepNoiPhuongTien_ThietBis_DS_ThietBis_ThietBiId",
                table: "GhepNoiPhuongTien_ThietBis");

            migrationBuilder.DropIndex(
                name: "IX_GhepNoiPhuongTien_ThietBis_ThietBi2Id",
                table: "GhepNoiPhuongTien_ThietBis");

            migrationBuilder.DropIndex(
                name: "IX_GhepNoiPhuongTien_ThietBis_ThietBiId",
                table: "GhepNoiPhuongTien_ThietBis");

            migrationBuilder.DropColumn(
                name: "ThietBi2Id",
                table: "GhepNoiPhuongTien_ThietBis");

            migrationBuilder.DropColumn(
                name: "ThietBiId",
                table: "GhepNoiPhuongTien_ThietBis");

            migrationBuilder.CreateIndex(
                name: "IX_GhepNoiPhuongTien_ThietBis_ThietBi_Id",
                table: "GhepNoiPhuongTien_ThietBis",
                column: "ThietBi_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GhepNoiPhuongTien_ThietBis_ThietBi2_Id",
                table: "GhepNoiPhuongTien_ThietBis",
                column: "ThietBi2_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GhepNoiPhuongTien_ThietBis_DS_ThietBis_ThietBi2_Id",
                table: "GhepNoiPhuongTien_ThietBis",
                column: "ThietBi2_Id",
                principalTable: "DS_ThietBis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GhepNoiPhuongTien_ThietBis_DS_ThietBis_ThietBi_Id",
                table: "GhepNoiPhuongTien_ThietBis",
                column: "ThietBi_Id",
                principalTable: "DS_ThietBis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GhepNoiPhuongTien_ThietBis_DS_ThietBis_ThietBi2_Id",
                table: "GhepNoiPhuongTien_ThietBis");

            migrationBuilder.DropForeignKey(
                name: "FK_GhepNoiPhuongTien_ThietBis_DS_ThietBis_ThietBi_Id",
                table: "GhepNoiPhuongTien_ThietBis");

            migrationBuilder.DropIndex(
                name: "IX_GhepNoiPhuongTien_ThietBis_ThietBi_Id",
                table: "GhepNoiPhuongTien_ThietBis");

            migrationBuilder.DropIndex(
                name: "IX_GhepNoiPhuongTien_ThietBis_ThietBi2_Id",
                table: "GhepNoiPhuongTien_ThietBis");

            migrationBuilder.AddColumn<Guid>(
                name: "ThietBi2Id",
                table: "GhepNoiPhuongTien_ThietBis",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ThietBiId",
                table: "GhepNoiPhuongTien_ThietBis",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GhepNoiPhuongTien_ThietBis_ThietBi2Id",
                table: "GhepNoiPhuongTien_ThietBis",
                column: "ThietBi2Id");

            migrationBuilder.CreateIndex(
                name: "IX_GhepNoiPhuongTien_ThietBis_ThietBiId",
                table: "GhepNoiPhuongTien_ThietBis",
                column: "ThietBiId");

            migrationBuilder.AddForeignKey(
                name: "FK_GhepNoiPhuongTien_ThietBis_DS_ThietBis_ThietBi2Id",
                table: "GhepNoiPhuongTien_ThietBis",
                column: "ThietBi2Id",
                principalTable: "DS_ThietBis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GhepNoiPhuongTien_ThietBis_DS_ThietBis_ThietBiId",
                table: "GhepNoiPhuongTien_ThietBis",
                column: "ThietBiId",
                principalTable: "DS_ThietBis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
