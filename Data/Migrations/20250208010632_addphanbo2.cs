using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addphanbo2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DS_PhuongTiens_DM_DonVis_DonVi_Id",
                table: "DS_PhuongTiens");

            migrationBuilder.DropIndex(
                name: "IX_DS_PhuongTiens_DonVi_Id",
                table: "DS_PhuongTiens");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DS_PhuongTiens_DonVi_Id",
                table: "DS_PhuongTiens",
                column: "DonVi_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DS_PhuongTiens_DM_DonVis_DonVi_Id",
                table: "DS_PhuongTiens",
                column: "DonVi_Id",
                principalTable: "DM_DonVis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
