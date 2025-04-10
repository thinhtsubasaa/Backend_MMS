using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addbophan6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PhuongTien_Id",
                table: "MMS_PhuTrachBoPhans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MMS_PhuTrachBoPhans_PhuongTien_Id",
                table: "MMS_PhuTrachBoPhans",
                column: "PhuongTien_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MMS_PhuTrachBoPhans_DS_PhuongTiens_PhuongTien_Id",
                table: "MMS_PhuTrachBoPhans",
                column: "PhuongTien_Id",
                principalTable: "DS_PhuongTiens",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MMS_PhuTrachBoPhans_DS_PhuongTiens_PhuongTien_Id",
                table: "MMS_PhuTrachBoPhans");

            migrationBuilder.DropIndex(
                name: "IX_MMS_PhuTrachBoPhans_PhuongTien_Id",
                table: "MMS_PhuTrachBoPhans");

            migrationBuilder.DropColumn(
                name: "PhuongTien_Id",
                table: "MMS_PhuTrachBoPhans");
        }
    }
}
