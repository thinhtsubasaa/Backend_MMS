using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addtb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Model_Id",
                table: "DS_ThietBis",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DS_ThietBis_Model_Id",
                table: "DS_ThietBis",
                column: "Model_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DS_ThietBis_DM_Models_Model_Id",
                table: "DS_ThietBis",
                column: "Model_Id",
                principalTable: "DM_Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DS_ThietBis_DM_Models_Model_Id",
                table: "DS_ThietBis");

            migrationBuilder.DropIndex(
                name: "IX_DS_ThietBis_Model_Id",
                table: "DS_ThietBis");

            migrationBuilder.DropColumn(
                name: "Model_Id",
                table: "DS_ThietBis");
        }
    }
}
