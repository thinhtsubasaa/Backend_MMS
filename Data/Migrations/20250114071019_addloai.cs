using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addloai : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Nhom_Id",
                table: "DM_Loais",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DM_Loais_Nhom_Id",
                table: "DM_Loais",
                column: "Nhom_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DM_Loais_DM_Nhoms_Nhom_Id",
                table: "DM_Loais",
                column: "Nhom_Id",
                principalTable: "DM_Nhoms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DM_Loais_DM_Nhoms_Nhom_Id",
                table: "DM_Loais");

            migrationBuilder.DropIndex(
                name: "IX_DM_Loais_Nhom_Id",
                table: "DM_Loais");

            migrationBuilder.DropColumn(
                name: "Nhom_Id",
                table: "DM_Loais");
        }
    }
}
