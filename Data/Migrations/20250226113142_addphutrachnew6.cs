using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addphutrachnew6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MMS_PhuTrachBoPhans_PhuongTien_Id",
                table: "MMS_PhuTrachBoPhans");

            migrationBuilder.AddColumn<Guid>(
                name: "PhuTrachBoPhan_Id",
                table: "DS_PhuongTiens",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MMS_PhuTrachBoPhans_PhuongTien_Id",
                table: "MMS_PhuTrachBoPhans",
                column: "PhuongTien_Id",
                unique: true,
                filter: "[PhuongTien_Id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MMS_PhuTrachBoPhans_PhuongTien_Id",
                table: "MMS_PhuTrachBoPhans");

            migrationBuilder.DropColumn(
                name: "PhuTrachBoPhan_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.CreateIndex(
                name: "IX_MMS_PhuTrachBoPhans_PhuongTien_Id",
                table: "MMS_PhuTrachBoPhans",
                column: "PhuongTien_Id");
        }
    }
}
