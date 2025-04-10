using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addbangkh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KeHoachBaoDuongs_DM_BaoDuongs_BaoDuong_Id",
                table: "KeHoachBaoDuongs");

            migrationBuilder.AddForeignKey(
                name: "FK_KeHoachBaoDuongs_DM_Models_BaoDuong_Id",
                table: "KeHoachBaoDuongs",
                column: "BaoDuong_Id",
                principalTable: "DM_Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KeHoachBaoDuongs_DM_Models_BaoDuong_Id",
                table: "KeHoachBaoDuongs");

            migrationBuilder.AddForeignKey(
                name: "FK_KeHoachBaoDuongs_DM_BaoDuongs_BaoDuong_Id",
                table: "KeHoachBaoDuongs",
                column: "BaoDuong_Id",
                principalTable: "DM_BaoDuongs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
