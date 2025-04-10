using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class lsbaoduongnew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichSuBaoDuongs_DM_BaoDuongs_BaoDuong_Id",
                table: "LichSuBaoDuongs");

            migrationBuilder.AddColumn<string>(
                name: "TrangThai",
                table: "LichSuBaoDuongs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuBaoDuongs_DM_Models_BaoDuong_Id",
                table: "LichSuBaoDuongs",
                column: "BaoDuong_Id",
                principalTable: "DM_Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LichSuBaoDuongs_DM_Models_BaoDuong_Id",
                table: "LichSuBaoDuongs");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "LichSuBaoDuongs");

            migrationBuilder.AddForeignKey(
                name: "FK_LichSuBaoDuongs_DM_BaoDuongs_BaoDuong_Id",
                table: "LichSuBaoDuongs",
                column: "BaoDuong_Id",
                principalTable: "DM_BaoDuongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
