using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Data.Migrations
{
    /// <inheritdoc />
    public partial class chinhsuadb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CBNV_DieuChuyen");

            migrationBuilder.DropTable(
                name: "ChiTiet_DV_PB_BPs");

            migrationBuilder.DropTable(
                name: "Menu_Roles");

            migrationBuilder.DropTable(
                name: "NhomDoiTacs");

            migrationBuilder.DropTable(
                name: "Role_DV_PBs");

            migrationBuilder.DropTable(
                name: "TaiXes");

            migrationBuilder.DropTable(
                name: "DieuChuyenNhanViens");

            migrationBuilder.DropTable(
                name: "Menus");

            migrationBuilder.DropTable(
                name: "RoleByDonVis");

            migrationBuilder.DropTable(
                name: "BoPhans");

            migrationBuilder.DropTable(
                name: "ChucVus");

            migrationBuilder.DropTable(
                name: "phongbans");

            migrationBuilder.DropTable(
                name: "DonVis");

            migrationBuilder.DropTable(
                name: "TapDoans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChiTiet_DV_PB_BPs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoPhan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChucVu_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DonVi_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    PhongBan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TapDoan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    User_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTiet_DV_PB_BPs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChucVus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MaChucVu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenChucVu = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChucVus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DonVi_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Parent_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhanMem_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhongBan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TapDoan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenMenu = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Url = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    isMoBi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NhomDoiTacs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MaNhomDoiTac = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenNhomDoiTac = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TenNhomDoiTac_EN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhomDoiTacs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleByDonVis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    User_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleByDonVis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleByDonVis_AspNetUsers_User_Id",
                        column: x => x.User_Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaiXes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HangBang = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MaTaiXe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoDienThoai = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenTaiXe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    isVaoCong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiXes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TapDoans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MaTapDoan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TapDoan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenTapDoan = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TapDoans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Menu_Roles",
                columns: table => new
                {
                    Menu_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Add = table.Column<bool>(type: "bit", nullable: false),
                    Cof = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Del = table.Column<bool>(type: "bit", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Edit = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Print = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    View = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menu_Roles", x => new { x.Menu_Id, x.Role_Id });
                    table.ForeignKey(
                        name: "FK_Menu_Roles_AspNetRoles_Role_Id",
                        column: x => x.Role_Id,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Menu_Roles_Menus_Menu_Id",
                        column: x => x.Menu_Id,
                        principalTable: "Menus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DonVis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TapDoan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DonVi_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MaDonVi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenDonVi = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonVis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonVis_TapDoans_TapDoan_Id",
                        column: x => x.TapDoan_Id,
                        principalTable: "TapDoans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "phongbans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DonVi_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MaPhongBan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhongBan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenPhongBan = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phongbans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_phongbans_DonVis_DonVi_Id",
                        column: x => x.DonVi_Id,
                        principalTable: "DonVis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BoPhans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhongBan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BoPhan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MaBoPhan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenBoPhan = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoPhans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoPhans_phongbans_PhongBan_Id",
                        column: x => x.PhongBan_Id,
                        principalTable: "phongbans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DieuChuyenNhanViens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoPhanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BoPhanNewId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChucVuId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChucVuNewId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DonViId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DonViNewId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhongbanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhongbanNewId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    User_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BoPhanNew_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BoPhan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChucVuNew_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChucVu_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DonViNew_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DonVi_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    MaDieuChuyen = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NgayDieuChuyen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PhongBanNew_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PhongBan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TapDoanNew_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TapDoan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    XacNhan = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DieuChuyenNhanViens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DieuChuyenNhanViens_AspNetUsers_User_Id",
                        column: x => x.User_Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DieuChuyenNhanViens_BoPhans_BoPhanId",
                        column: x => x.BoPhanId,
                        principalTable: "BoPhans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DieuChuyenNhanViens_BoPhans_BoPhanNewId",
                        column: x => x.BoPhanNewId,
                        principalTable: "BoPhans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DieuChuyenNhanViens_ChucVus_ChucVuId",
                        column: x => x.ChucVuId,
                        principalTable: "ChucVus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DieuChuyenNhanViens_ChucVus_ChucVuNewId",
                        column: x => x.ChucVuNewId,
                        principalTable: "ChucVus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DieuChuyenNhanViens_DonVis_DonViId",
                        column: x => x.DonViId,
                        principalTable: "DonVis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DieuChuyenNhanViens_DonVis_DonViNewId",
                        column: x => x.DonViNewId,
                        principalTable: "DonVis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DieuChuyenNhanViens_phongbans_PhongbanId",
                        column: x => x.PhongbanId,
                        principalTable: "phongbans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DieuChuyenNhanViens_phongbans_PhongbanNewId",
                        column: x => x.PhongbanNewId,
                        principalTable: "phongbans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Role_DV_PBs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoPhan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DonVi_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Phongban_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RoleByDonVi_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TapDoan_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role_DV_PBs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Role_DV_PBs_BoPhans_BoPhan_Id",
                        column: x => x.BoPhan_Id,
                        principalTable: "BoPhans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Role_DV_PBs_DonVis_DonVi_Id",
                        column: x => x.DonVi_Id,
                        principalTable: "DonVis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Role_DV_PBs_RoleByDonVis_RoleByDonVi_Id",
                        column: x => x.RoleByDonVi_Id,
                        principalTable: "RoleByDonVis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Role_DV_PBs_TapDoans_TapDoan_Id",
                        column: x => x.TapDoan_Id,
                        principalTable: "TapDoans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Role_DV_PBs_phongbans_Phongban_Id",
                        column: x => x.Phongban_Id,
                        principalTable: "phongbans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CBNV_DieuChuyen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DieuChuyenNhanVien_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DonViTraLuongId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DonViTraLuongNewId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    User_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DonViTraLuongNew_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DonViTraLuong_Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CBNV_DieuChuyen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CBNV_DieuChuyen_AspNetUsers_User_Id",
                        column: x => x.User_Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CBNV_DieuChuyen_DieuChuyenNhanViens_DieuChuyenNhanVien_Id",
                        column: x => x.DieuChuyenNhanVien_Id,
                        principalTable: "DieuChuyenNhanViens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CBNV_DieuChuyen_DonVis_DonViTraLuongId",
                        column: x => x.DonViTraLuongId,
                        principalTable: "DonVis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CBNV_DieuChuyen_DonVis_DonViTraLuongNewId",
                        column: x => x.DonViTraLuongNewId,
                        principalTable: "DonVis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoPhans_PhongBan_Id",
                table: "BoPhans",
                column: "PhongBan_Id");

            migrationBuilder.CreateIndex(
                name: "IX_CBNV_DieuChuyen_DieuChuyenNhanVien_Id",
                table: "CBNV_DieuChuyen",
                column: "DieuChuyenNhanVien_Id");

            migrationBuilder.CreateIndex(
                name: "IX_CBNV_DieuChuyen_DonViTraLuongId",
                table: "CBNV_DieuChuyen",
                column: "DonViTraLuongId");

            migrationBuilder.CreateIndex(
                name: "IX_CBNV_DieuChuyen_DonViTraLuongNewId",
                table: "CBNV_DieuChuyen",
                column: "DonViTraLuongNewId");

            migrationBuilder.CreateIndex(
                name: "IX_CBNV_DieuChuyen_User_Id",
                table: "CBNV_DieuChuyen",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DieuChuyenNhanViens_BoPhanId",
                table: "DieuChuyenNhanViens",
                column: "BoPhanId");

            migrationBuilder.CreateIndex(
                name: "IX_DieuChuyenNhanViens_BoPhanNewId",
                table: "DieuChuyenNhanViens",
                column: "BoPhanNewId");

            migrationBuilder.CreateIndex(
                name: "IX_DieuChuyenNhanViens_ChucVuId",
                table: "DieuChuyenNhanViens",
                column: "ChucVuId");

            migrationBuilder.CreateIndex(
                name: "IX_DieuChuyenNhanViens_ChucVuNewId",
                table: "DieuChuyenNhanViens",
                column: "ChucVuNewId");

            migrationBuilder.CreateIndex(
                name: "IX_DieuChuyenNhanViens_DonViId",
                table: "DieuChuyenNhanViens",
                column: "DonViId");

            migrationBuilder.CreateIndex(
                name: "IX_DieuChuyenNhanViens_DonViNewId",
                table: "DieuChuyenNhanViens",
                column: "DonViNewId");

            migrationBuilder.CreateIndex(
                name: "IX_DieuChuyenNhanViens_PhongbanId",
                table: "DieuChuyenNhanViens",
                column: "PhongbanId");

            migrationBuilder.CreateIndex(
                name: "IX_DieuChuyenNhanViens_PhongbanNewId",
                table: "DieuChuyenNhanViens",
                column: "PhongbanNewId");

            migrationBuilder.CreateIndex(
                name: "IX_DieuChuyenNhanViens_User_Id",
                table: "DieuChuyenNhanViens",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DonVis_TapDoan_Id",
                table: "DonVis",
                column: "TapDoan_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_Roles_Role_Id",
                table: "Menu_Roles",
                column: "Role_Id");

            migrationBuilder.CreateIndex(
                name: "IX_phongbans_DonVi_Id",
                table: "phongbans",
                column: "DonVi_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Role_DV_PBs_BoPhan_Id",
                table: "Role_DV_PBs",
                column: "BoPhan_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Role_DV_PBs_DonVi_Id",
                table: "Role_DV_PBs",
                column: "DonVi_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Role_DV_PBs_Phongban_Id",
                table: "Role_DV_PBs",
                column: "Phongban_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Role_DV_PBs_RoleByDonVi_Id",
                table: "Role_DV_PBs",
                column: "RoleByDonVi_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Role_DV_PBs_TapDoan_Id",
                table: "Role_DV_PBs",
                column: "TapDoan_Id");

            migrationBuilder.CreateIndex(
                name: "IX_RoleByDonVis_User_Id",
                table: "RoleByDonVis",
                column: "User_Id");
        }
    }
}
