using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class addghepnoi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GhepNoiPhuongTien_ThietBis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoPhan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DonVi_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhuongTien_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThietBi_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThietBiId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThietBi2_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ThietBi2Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenThietBi = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    table.PrimaryKey("PK_GhepNoiPhuongTien_ThietBis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GhepNoiPhuongTien_ThietBis_DS_PhuongTiens_PhuongTien_Id",
                        column: x => x.PhuongTien_Id,
                        principalTable: "DS_PhuongTiens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GhepNoiPhuongTien_ThietBis_DS_ThietBis_ThietBi2Id",
                        column: x => x.ThietBi2Id,
                        principalTable: "DS_ThietBis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GhepNoiPhuongTien_ThietBis_DS_ThietBis_ThietBiId",
                        column: x => x.ThietBiId,
                        principalTable: "DS_ThietBis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GhepNoiPhuongTien_ThietBis_PhuongTien_Id",
                table: "GhepNoiPhuongTien_ThietBis",
                column: "PhuongTien_Id");

            migrationBuilder.CreateIndex(
                name: "IX_GhepNoiPhuongTien_ThietBis_ThietBi2Id",
                table: "GhepNoiPhuongTien_ThietBis",
                column: "ThietBi2Id");

            migrationBuilder.CreateIndex(
                name: "IX_GhepNoiPhuongTien_ThietBis_ThietBiId",
                table: "GhepNoiPhuongTien_ThietBis",
                column: "ThietBiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GhepNoiPhuongTien_ThietBis");
        }
    }
}
