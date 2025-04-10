using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class lsbaoduong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayTao",
                table: "KeHoachBaoDuongs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LichSuBaoDuongs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BaoDuong_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhuongTien_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThietBi_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KetQua = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ngay = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuBaoDuongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuBaoDuongs_DM_BaoDuongs_BaoDuong_Id",
                        column: x => x.BaoDuong_Id,
                        principalTable: "DM_BaoDuongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuBaoDuongs_DS_PhuongTiens_PhuongTien_Id",
                        column: x => x.PhuongTien_Id,
                        principalTable: "DS_PhuongTiens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuBaoDuongs_DS_ThietBis_ThietBi_Id",
                        column: x => x.ThietBi_Id,
                        principalTable: "DS_ThietBis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LichSuBaoDuongs_BaoDuong_Id",
                table: "LichSuBaoDuongs",
                column: "BaoDuong_Id");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuBaoDuongs_PhuongTien_Id",
                table: "LichSuBaoDuongs",
                column: "PhuongTien_Id");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuBaoDuongs_ThietBi_Id",
                table: "LichSuBaoDuongs",
                column: "ThietBi_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LichSuBaoDuongs");

            migrationBuilder.DropColumn(
                name: "NgayTao",
                table: "KeHoachBaoDuongs");
        }
    }
}
