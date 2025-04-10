using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addbangtansuat2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TanSuat_Id",
                table: "DM_Models",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DM_Models_TanSuat_Id",
                table: "DM_Models",
                column: "TanSuat_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DM_Models_DM_TanSuats_TanSuat_Id",
                table: "DM_Models",
                column: "TanSuat_Id",
                principalTable: "DM_TanSuats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DM_Models_DM_TanSuats_TanSuat_Id",
                table: "DM_Models");

            migrationBuilder.DropIndex(
                name: "IX_DM_Models_TanSuat_Id",
                table: "DM_Models");

            migrationBuilder.DropColumn(
                name: "TanSuat_Id",
                table: "DM_Models");
        }
    }
}
