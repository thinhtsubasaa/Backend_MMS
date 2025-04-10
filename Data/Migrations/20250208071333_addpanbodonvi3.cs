using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addpanbodonvi3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BoPhan_Id",
                table: "LichSuPhanBoDonVis",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BoPhan_Id",
                table: "DS_PhuongTiens",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoPhan_Id",
                table: "LichSuPhanBoDonVis");

            migrationBuilder.DropColumn(
                name: "BoPhan_Id",
                table: "DS_PhuongTiens");
        }
    }
}
