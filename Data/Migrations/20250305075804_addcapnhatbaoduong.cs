using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addcapnhatbaoduong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DM_HangMucs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TanSuat_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NoiDungBaoDuong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DinhMuc = table.Column<int>(type: "int", nullable: false),
                    LoaiBaoDuong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_DM_HangMucs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DM_HangMucs_DM_TanSuats_TanSuat_Id",
                        column: x => x.TanSuat_Id,
                        principalTable: "DM_TanSuats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LichSuBaoDuong_ChiTiets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LichSuBaoDuong_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhuongTien_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HangMuc_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HinhAnh = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_LichSuBaoDuong_ChiTiets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuBaoDuong_ChiTiets_DM_HangMucs_HangMuc_Id",
                        column: x => x.HangMuc_Id,
                        principalTable: "DM_HangMucs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuBaoDuong_ChiTiets_DS_PhuongTiens_PhuongTien_Id",
                        column: x => x.PhuongTien_Id,
                        principalTable: "DS_PhuongTiens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuBaoDuong_ChiTiets_LichSuBaoDuongs_LichSuBaoDuong_Id",
                        column: x => x.LichSuBaoDuong_Id,
                        principalTable: "LichSuBaoDuongs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThongTinTheoHangMucs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhuongTien_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HangMuc_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GiaTriBaoDuong = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_ThongTinTheoHangMucs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThongTinTheoHangMucs_DM_HangMucs_HangMuc_Id",
                        column: x => x.HangMuc_Id,
                        principalTable: "DM_HangMucs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThongTinTheoHangMucs_DS_PhuongTiens_PhuongTien_Id",
                        column: x => x.PhuongTien_Id,
                        principalTable: "DS_PhuongTiens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DM_HangMucs_TanSuat_Id",
                table: "DM_HangMucs",
                column: "TanSuat_Id");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuBaoDuong_ChiTiets_HangMuc_Id",
                table: "LichSuBaoDuong_ChiTiets",
                column: "HangMuc_Id");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuBaoDuong_ChiTiets_LichSuBaoDuong_Id",
                table: "LichSuBaoDuong_ChiTiets",
                column: "LichSuBaoDuong_Id");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuBaoDuong_ChiTiets_PhuongTien_Id",
                table: "LichSuBaoDuong_ChiTiets",
                column: "PhuongTien_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ThongTinTheoHangMucs_HangMuc_Id",
                table: "ThongTinTheoHangMucs",
                column: "HangMuc_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ThongTinTheoHangMucs_PhuongTien_Id",
                table: "ThongTinTheoHangMucs",
                column: "PhuongTien_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LichSuBaoDuong_ChiTiets");

            migrationBuilder.DropTable(
                name: "ThongTinTheoHangMucs");

            migrationBuilder.DropTable(
                name: "DM_HangMucs");
        }
    }
}
