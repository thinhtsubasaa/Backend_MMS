using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using static ERP.Commons;

namespace ERP.Models
{

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
    }
    public class LoginMobileModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public Guid DonVi_Id { get; set; }

    }

    public class CheckRaCong
    {
        public string SoKhung { get; set; }
        public bool Check { get; set; }
        public string MaPin { get; set; }

    }
    public class InfoLogin
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime? Expires { get; set; }
        public bool MustChangePass { get; set; }
        public string AccessRole { get; set; }
        public string HinhAnhUrl { get; set; }
        public string QrCode { get; set; }
        public string MaNhanVien { get; set; }
        public string TenPhongBan { get; set; }
        public string CongBaoVe { get; set; }
        public string ChuoiPhongBan { get; set; }
        public string ChucDanh { get; set; }
        public string ChucVu { get; set; }
        public string TrangThai { get; set; }

    }
    public class RegisterModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }
        [RegularExpression(@"^\d{1,10}$", ErrorMessage = "Mã nhân viên chỉ được chứa số và có độ dài nhỏ hơn hoặc bằng 10 chữ số.")]
        public string MaNhanVien { get; set; }
        public string ChucDanh { get; set; }

        public string FullName { get; set; }
        public List<string> RoleNames { get; set; }
        public bool IsActive { get; set; }
        public bool? AccountLevel { get; set; }
        public string MaPin { get; set; }
        public Guid? DonViTraLuong_Id { get; set; }
        public List<ClassChiTiet_DV_PB_BP> chiTiet { get; set; }

    }
    public class ClassChiTiet_DV_PB_BP
    {
        public Guid Id { get; set; }
        public Guid User_Id { get; set; }
        public Guid? TapDoan_Id { get; set; }
        public Guid? DonVi_Id { get; set; }
        public Guid? BoPhan_Id { get; set; }
        public Guid? PhongBan_Id { get; set; }
        public Guid? ChucVu_Id { get; set; }
    }
    public class ChangePasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string Password { get; set; }
        [Required]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [Display(Name = "Mật khẩu xác nhận")]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu mới không đúng.")]
        public string ConfirmNewPassword { get; set; }
    }

    public class ListUserModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string MaNhanVien { get; set; }
        public string FullName { get; set; }
        public string ChucDanh { get; set; }
        public string TenDonVi { get; set; }
        public string TenBoPhan { get; set; }
        public string TenChucVu { get; set; }
        public string TenPhongBan { get; set; }
        public Guid? DonViTraLuong_Id { get; set; }
        public string TenDonViTraLuong { get; set; }

    }
    public class ImportDanhSachXeDongContModel
    {
        public Guid? Id { get; set; }
        public string STT { get; set; }
        public Guid KhoThanhPham_Id { get; set; }
        public string LoaiXe { get; set; }

        public string SoKhung { get; set; }

        public string SoMay { get; set; }
        public string MaMau { get; set; }
        public Guid? TaiXe_Id { get; set; }
        public string TaiXe { get; set; }
        public Guid? DongCont_Id { get; set; }
        public string SoCont { get; set; }
        public string SoSeal { get; set; }

        public string YeuCau { get; set; }
        public string NoiGiao { get; set; }


        public string TrungSoCont { get; set; }
        public string TrungSoSeal { get; set; }

        public string NgayDongCont { get; set; }
        public bool IsLoi { get; set; }

        public List<string> lst_Lois { get; set; }
    }
    public class ImportQuanLyBienTamModel
    {
        public Guid? Id { get; set; }
        public string STT { get; set; }
        public string SoSei { get; set; }
        public string MaHoSo { get; set; }

        public string BienSo { get; set; }
        public string SoChoNgoi { get; set; }
        public string NoiDi { get; set; }
        public string NoiDen { get; set; }
        public string SoMay { get; set; }
        public string SoKhung { get; set; }
        public string NhanHieu { get; set; }
        public string SoLoai { get; set; }
        public string Ngay { get; set; }
        public bool IsLoi { get; set; }
        public List<string> lst_Lois { get; set; }
    }
    public class ImportXeCPUModel
    {
        public Guid? Id { get; set; }
        public string STT { get; set; }
        public string SoKhung { get; set; }
        public string SoMay { get; set; }
        public Guid SanPham_Id { get; set; }
        public string TenSanPham { get; set; }
        public string TenThuongMai { get; set; }
        public string MaSanPham { get; set; }
        public Guid? MauXe_Id { get; set; }
        public string MaMauSon { get; set; }
        public string TenMauSon { get; set; }

        public string MaBinh { get; set; }
        public bool IsLoi { get; set; }
        public List<string> lst_Lois { get; set; }
    }
    public class ImportDMDongContModel
    {
        public Guid? Id { get; set; }
        public string STT { get; set; }
        public string MaSoCont { get; set; }
        public string SoCont { get; set; }
        public bool TinhTrang { get; set; }
        public bool IsLoi { get; set; }
        public bool IsDeleted { get; set; }
        public List<string> lst_Lois { get; set; }
    }
    public class ImportDSTheNV
    {
        public Guid? Id { get; set; }
        public string STT { get; set; }
        public string Msnv { get; set; }
        public string Hovaten { get; set; }
        public string PhongBan { get; set; }
        public string HinhAnh { get; set; }
        public string QrCode { get; set; }
        public bool IsLoi { get; set; }
        public List<string> lst_Lois { get; set; }
    }

    public class ImportMMS_DS_PhuongTien
    {

        public Guid Id { get; set; }
        public string BienSo1 { get; set; }
        public string BienSo2 { get; set; }
        public string DonViSuDung { get; set; }
        public string HinhAnh { get; set; }
        public string TinhTrang { get; set; }
        public string NgayBatDau { get; set; }
        public string Note { get; set; }
        public string LoaiPT { get; set; }
        public string Model { get; set; }
        public string Model_Option { get; set; }
        public string KLBT { get; set; }
        public string KLHH { get; set; }
        public string KLTB { get; set; }
        public string KLKT { get; set; }
        public string Address_nearest { get; set; }
        public string ViTri { get; set; }
        public string ViTri_Lat { get; set; }
        public string ViTri_Long { get; set; }
        public string MaPhuongTien { get; set; }

        public bool IsLoi { get; set; }
        public List<string> lst_Lois { get; set; }
    }

    public class ImportMMS_DS_ThietBi
    {
        public Guid Id { get; set; }
        public string MaThietBi { get; set; }
        public string MaCode_BienSo1 { get; set; }
        public string MaCode_BienSo2 { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public string LoaiTB { get; set; }
        public string PhanBo { get; set; }
        public string ViTri { get; set; }
        public string ViTri_Lat { get; set; }
        public string ViTri_Long { get; set; }
        public string TinhTrang { get; set; }
        public string NgayBatDau { get; set; }
        public string Note { get; set; }
        public bool IsLoi { get; set; }
        public List<string> lst_Lois { get; set; }
    }

    public class ImportMMS_DM_DonVi
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Name_Eng { get; set; }
        public string Note { get; set; }
        public string MaDV { get; set; }
        public bool IsLoi { get; set; }
        public List<string> lst_Lois { get; set; }
    }
    public class ImportMMS_DM_BoPhan
    {
        public Guid Id { get; set; }

        public Guid? DonVi_Id { get; set; }

        public string Name { get; set; }
        public string Name_Eng { get; set; }
        public string Note { get; set; }
        public string MaBP { get; set; }
        public bool IsLoi { get; set; }
        public List<string> lst_Lois { get; set; }
    }
      public class ImportMMS_DM_Nhom
    {
       public Guid Id { get; set; }
        public string Name { get; set; }
        public string Name_Eng { get; set; }
        public string Note { get; set; }        
        public bool IsLoi { get; set; }
        public List<string> lst_Lois { get; set; }
    }
     public class ImportMMS_DM_Loai
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Name_Eng { get; set; }
        public string ThuocNhom { get; set; }
        public string Note { get; set; }   
        public bool IsLoi { get; set; }
        public List<string> lst_Lois { get; set; }
    }
     public class ImportMMS_DM_Model
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Option { get; set; }
        public string Code { get; set; }
        public string Name_Eng { get; set; }
        public string Type { get; set; }
        public string PairingAbility { get; set; }
        public string KLBT { get; set; }
        public string TTMK_KLHH { get; set; }
        public string KLTB { get; set; }
        public string KLKT { get; set; }
        public string Note { get; set; }

        public bool IsLoi { get; set; }
        public List<string> lst_Lois { get; set; }
    }
     public class ImportMMS_DM_TinhTrang
    {
         public Guid Id { get; set; }
        public string Name { get; set; }
        public string Arrange { get; set; }

        public bool IsLoi { get; set; }
        public List<string> lst_Lois { get; set; }
    }
    public class ImportMMS_Status
    {
         public Guid Id { get; set; }
        public string Name { get; set; }
        public string Note { get; set; }

        public bool IsLoi { get; set; }
        public List<string> lst_Lois { get; set; }
    }
    public class ImportKeHoachGiaoXeModel
    {
        public Guid? Id { get; set; }
        public string STT { get; set; }
        public string SoPhieu { get; set; }
        public string DongXe { get; set; }
        public Guid? SanPham_Id { get; set; }
        public string LoaiXe { get; set; }

        public string SoKhung { get; set; }
        public string SoMay { get; set; }
        public Guid? MauSac_Id { get; set; }
        public string MaMauSon { get; set; }
        public string TenMauSon { get; set; }
        public Guid? VanChuyen_Id { get; set; }
        public string PhuongThuc { get; set; }
        public Guid? DoiTac_Id { get; set; }
        public string TenDoiTac { get; set; }
        public Guid? YeuCau_Id { get; set; }

        public string MaYeuCau { get; set; }
        public string TenYeuCau { get; set; }
        public Guid? NoiDi_Id { get; set; }
        public string MaNoiDi { get; set; }
        public string TenNoiDi { get; set; }
        public Guid? NoiDen_Id { get; set; }
        public string MaNoiDen { get; set; }
        public string TenNoiDen { get; set; }
        public string KVDen { get; set; }
        public Guid? TaiXe_Id { get; set; }
        public string TenTaiXe { get; set; }
        public Guid? TMS_DanhSachPhuongTien_Id { get; set; }
        public string BienSo { get; set; }
        public string MaNhanVien { get; set; }
        public string NgayYeuCau { get; set; }
        public string SoDienThoai { get; set; }
        public string NgayGiao { get; set; }
        public string MaKV { get; set; }
        public string TenKV { get; set; }
        public Guid? KVDen_Id { get; set; }
        public string NgayDenKV { get; set; }
        public string GioDenKV { get; set; }
        public string NgayDuKienDen { get; set; }
        public string GioDen { get; set; }
        public string SoCont { get; set; }
        public string SoSeal { get; set; }
        public string SoXe { get; set; }
        public string Tau { get; set; }
        public string GhiChu { get; set; }
        public string TinhTrangXe { get; set; }
        public string XeCong { get; set; }
        public string MaBinh { get; set; }
        public string AC { get; set; }
        public string CS { get; set; }
        public string Thung { get; set; }
        public string SeriThung { get; set; }
        public string NgaySoTam { get; set; }
        public string DiemSoHuu { get; set; }
        public string NgayTao { get; set; }
        public bool IsLoi { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public Guid CreatedBy { get; set; }
        // public List<ImportKVChuyenTiepModal> KVChuyenTieps { get; set; }
        public List<string> lst_Lois { get; set; }
    }
    public class ImportKVChuyenTiepModal
    {

        public Guid? Id { get; set; }
        public Guid? KeHoachGiaoXe_Id { get; set; }
        public Guid? KhuVucChuyenTiepDiaDiem_Id { get; set; }
        public string MaKV { get; set; }
        public string TenKV { get; set; }
    }
    public class ImportBienSoTamModel
    {

        public Guid? Id { get; set; }
        public string SoKhung { get; set; }
        public string SoMay { get; set; }
        public string TenChuXe { get; set; }
        public string DiaChi { get; set; }
        public string BienSo { get; set; }
        public string PhamViHoatDong { get; set; }
        public string NhanHieu { get; set; }
        public string SoLoai { get; set; }
        public string SoChoNgoi { get; set; }
        public string LoaiXe { get; set; }
        public string TenMau { get; set; }
        public string NgayDangKy { get; set; }
        public string GiaTriDenNgay { get; set; }
        public bool IsLoi { get; set; }
        // public List<ImportKVChuyenTiepModal> KVChuyenTieps { get; set; }
        public List<string> lst_Lois { get; set; }
    }
    public class PhienBanAddModel
    {
        private string _MaPhienBan;
        public string MaPhienBan { get { return _MaPhienBan; } set { _MaPhienBan = value.Trim(); } }
        private string _MoTa;
        public string MoTa { get { return _MoTa; } set { _MoTa = value.Trim(); } }
        private string _GhiChu;
        public string GhiChu { get { return _GhiChu; } set { _GhiChu = value?.Trim(); } }
        public bool IsSuDung { get; set; }
        public string File_Name { get; set; }
        public string File_Url { get; set; }
    }
    public class ImportCauHinhDonHangChiTietModel
    {
        public Guid? Id { get; set; }
        public string STT { get; set; }
        public string SoTBGX { get; set; }
        public Guid? DonHang_Id { get; set; }
        public string LoaiXe { get; set; }
        public string SoKhung { get; set; }
        public string SoMay { get; set; }
        public string Thung { get; set; }
        public string MucDich { get; set; }
        public string MaSanPham { get; set; }
        public bool IsNhanXe { get; set; }
        public bool IsNhapKho { get; set; }
        public string MaMauSon { get; set; }
        public string TenMauSon { get; set; }
        public Guid? NoiDi_Id { get; set; }
        public string MaNoiDi { get; set; }
        public string TenNoiDi { get; set; }
        public Guid? NoiDen_Id { get; set; }
        public string MaNoiDen { get; set; }
        public string TenNoiDen { get; set; }
        public string XeDonHang { get; set; }
        public string NgayDiDuKien { get; set; }
        public string NgayDenDuKien { get; set; }
        public string NgayDuKienNhanXe { get; set; }
        public string GhiChu { get; set; }
        public string TenPhuongThucVanChuyen { get; set; }
        public string ThoiGianYCDen { get; set; }
        public bool IsLoi { get; set; }
        public bool IsTrung { get; set; }
        public bool IsDeleted { get; set; }
        public Guid SanPham_Id { get; set; }
        public Guid? DonVi_Id { get; set; }
        public Guid? PhuongThucVanChuyen_Id { get; set; }
        public Guid? MauSac_Id { get; set; }
        public string SoHopDong { get; set; }
        public List<string> lst_Lois { get; set; }
    }
    public class ImportDonHangNhanXeModel
    {
        public Guid? Id { get; set; }
        public string STT { get; set; }
        public string LoaiXe { get; set; }
        public string SoKhung { get; set; }
        public string SoMay { get; set; }
        public string KVTheoMien { get; set; }
        public string Mien { get; set; }
        public string ThuocDonHang { get; set; }

        public string ThieuCongNo { get; set; }

        public string NhaMayGiaoXe { get; set; }
        public string NgayDLNhanXe { get; set; }

        public bool IsNhanXe { get; set; }

        public string MaMauSon { get; set; }
        public string TenMauSon { get; set; }

        public Guid? NoiDen_Id { get; set; }
        public string MaNoiDen { get; set; }
        public string TenNoiDen { get; set; }

        public string TenPhuongThucVanChuyen { get; set; }

        public bool IsLoi { get; set; }
        public bool IsTrung { get; set; }
        public Guid SanPham_Id { get; set; }

        public Guid? PhuongThucVanChuyen_Id { get; set; }
        public Guid? MauSac_Id { get; set; }

        public List<string> lst_Lois { get; set; }
    }
    public class ImportCauHinhDonHangModel
    {

        public Guid? Id { get; set; }
        public string Name { get; set; }
        public string SoDonHang { get; set; }
        public string NgayDonHang { get; set; }
        public string NoiGui { get; set; }
        public Guid? NoiGui_Id { get; set; }
        public string NoiNhan { get; set; }
        public Guid? NoiNhan_Id { get; set; }
        public string NguoiGui { get; set; }
        public string NguoiNhan { get; set; }

        public bool IsDeleted { get; set; }
        public string GhiChu { get; set; }
        public List<ImportCauHinhDonHangChiTietModel> CHDHCT { get; set; }
    }
    public class DongSanPhamAddModel
    {
        private string _MaDongSanPham;
        public string MaDongSanPham { get { return _MaDongSanPham; } set { _MaDongSanPham = value.Trim(); } }
        private string _TenDongSanPham;
        public string TenDongSanPham { get { return _TenDongSanPham; } set { _TenDongSanPham = value.Trim(); } }
        public Guid DonVi_Id { get; set; }
        public Guid NhanHieu_Id { get; set; }
        public bool IsSuDung { get; set; }
    }
    public class PhuKienAddModel
    {
        private string _MaPhuKien;
        public string MaPhuKien { get { return _MaPhuKien; } set { _MaPhuKien = value.Trim(); } }
        private string _TenPhuKien;
        public string TenPhuKien { get { return _TenPhuKien; } set { _TenPhuKien = value.Trim(); } }
        // public Guid DonVi_Id { get; set; }
        public Guid DonViTinh_Id { get; set; }
        public bool IsSuDung { get; set; }
    }
    public class ListUserModelNghiViec
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string MaNhanVien { get; set; }
        public string FullName { get; set; }
        public string ChucDanh { get; set; }
        public string TenDonVi { get; set; }
        public string TenBoPhan { get; set; }
        public string TenChucVu { get; set; }
        public string TenPhongBan { get; set; }
        public Guid? DonViTraLuong_Id { get; set; }
        public string TenDonViTraLuong { get; set; }
        public string NgayNghiViec { get; set; }
        public string PhoneNumber { get; set; }
        public string GhiChu { get; set; }

    }
    public class LoaiXeAddModel
    {
        private string _MaLoaiSanPham;
        public string MaLoaiSanPham { get { return _MaLoaiSanPham; } set { _MaLoaiSanPham = value.Trim(); } }
        private string _TenLoaiSanPham;
        public string TenLoaiSanPham { get { return _TenLoaiSanPham; } set { _TenLoaiSanPham = value.Trim(); } }
        public Guid DongSanPham_Id { get; set; }
        public bool IsSuDung { get; set; }
    }
    public class CBNVInfoModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string MaNhanVien { get; set; }

        public string UserName { get; set; }
        public string FullName { get; set; }
        public string ChucDanh { get; set; }
        public Guid? DonVi_Id { get; set; }
        public Guid? BoPhan_Id { get; set; }
        public string TenDonVi { get; set; }
        public string TenBoPhan { get; set; }
        public Guid? ChucVu_Id { get; set; }
        public Guid? PhongBan_Id { get; set; }
        public string TenChucVu { get; set; }
        public string TenPhongBan { get; set; }
        public Guid? DonViTraLuong_Id { get; set; }
        public string TenDonViTraLuong { get; set; }
        public bool NghiViec { get; set; }
        public DateTime? NgayNghiViec { get; set; }
        public string GhiChu { get; set; }
        public bool? AccountLevel { get; set; }
    }
    public class ThongTinXe_XeQuaThilogi
    {

        public DateTime NgayNhan { get; set; }
        public string NguoiNhan { get; set; }
        public string NoiNhan { get; set; }
        public string ToaDo { get; set; }

    }
    public class TinhTrangDonHangXe
    {

        public bool IsNhanXe { get; set; }
        public bool IsNhapKho { get; set; }
        public bool IsXuatKho { get; set; }
        public bool IsDaGiao { get; set; }
        public string ToaDo { get; set; }

    }
    public class ThongTinXe_LichSuChuyenKho
    {
        public string Kho { get; set; }
        public string BaiXe { get; set; }
        public string ToaDo { get; set; }
        public string ViTri { get; set; }

        public string NguoiNhapBai { get; set; }
        public string ThoiGianVao { get; set; }
        public string ThoiGianRa { get; set; }
        public int SoNgay { get; set; }
        public DateTime NgayVao { get; set; }
        public DateTime? NgayRa { get; set; }
    }
    public class ThongTinXe_LichSuXuatKho
    {
        public string Thongtinvanchuyen { get; set; }
        public string ThongTinChiTiet { get; set; }
        public string ToaDo { get; set; }
        public string ThongtinMap { get; set; }
        public Guid? DiaDiemDen_Id { get; set; }
        public string DiaDiemDen { get; set; }
        public string NguoiPhuTrach { get; set; }
        public string TrangThai { get; set; }
        public string SoCont { get; set; }
        public string SoSeal { get; set; }
        public string Tau { get; set; }
        public string NoiRutCont { get; set; }
        public string Ngay { get; set; }
    }

    public class ThongTinXe_LichSuGiaoXe
    {
        public string soTBGX { get; set; }
        public string NoiGiao { get; set; }
        public string ToaDo { get; set; }
        public string NguoiPhuTrach { get; set; }
        public string Ngay { get; set; }
    }

    public class BaoCaoThongTinXe
    {
        public string LoaiPhuongTien { get; set; }
        public string NhanHieu { get; set; }
        public string DongSanPham { get; set; }
        public string LoaiSanPham { get; set; }
        public string SanPham { get; set; }
        public string SoHopDong { get; set; }
        public string SoBill { get; set; }
        public string SoKien { get; set; }
        public string SoKhung { get; set; }
        public string SoMay { get; set; }
        public string SoBody { get; set; }
        public string MauSon { get; set; }
        public string Thung { get; set; }
        public string ModelThung { get; set; }
        public string SoVan { get; set; }
        public string SoLOT { get; set; }
    }
    public class NghiViecModel
    {
        public string Id { get; set; }
        public DateTime NgayNghiViec { get; set; }
        public string GhiChu { get; set; }
        public string GhiChuImport { get; set; }

    }
    public class UserInfoModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string MaNhanVien { get; set; }
        public string HinhAnhUrl { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public List<string> RoleNames { get; set; }
        public string ChucDanh { get; set; }
        public Guid? DonViTraLuong_Id { get; set; }
        public string TenDonViTraLuong { get; set; }
        public bool NghiViec { get; set; }
        public DateTime NgayNghiViec { get; set; }
        public string GhiChu { get; set; }
        public string MaPin { get; set; }
        public bool? AccountLevel { get; set; }
        public List<ClassChiTiet_UserInfoModel> chiTiet { get; set; }

    }
    public class ClassChiTiet_UserInfoModel
    {
        public Guid? PhongBan_Id { get; set; }
        public string TenPhongBan { get; set; }
        public Guid? DonVi_Id { get; set; }
        public Guid? BoPhan_Id { get; set; }
        public string TenDonVi { get; set; }
        public string TenBoPhan { get; set; }
        public Guid? ChucVu_Id { get; set; }
        public string TenChucVu { get; set; }
        public Guid Id { get; set; }
        public Guid User_Id { get; set; }
        public Guid? TapDoan_Id { get; set; }
        public string TenTapDoan { get; set; }
    }

    public class FileDinhKem
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string FileLink { get; set; }
        public string FileData { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }
    public class UserToken
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool MustChangePass { get; set; }
        public string AccessRole { get; set; }
        public string HinhAnhUrl { get; set; }


    }

    public class XeChoRaCong
    {
        public Guid? Id { get; set; }
        public string BenVanChuyen { get; set; }
        public string BienSo { get; set; }
        public string SoKhung { get; set; }
        public string MauXe { get; set; }
        public string TenTaiXe { get; set; }
        public string NgayDiChuyen { get; set; }
        public string BaiXe { get; set; }
    }
    public class FileModel
    {
        public Guid? id { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public bool IsRemoved { get; set; }
    }
    public class TuKhoaModel
    {
        public Guid? id { get; set; }
        public string TenTuKhoa { get; set; }
        public bool IsRemoved { get; set; }
    }
    public class PhanQuyenDonVi
    {
        public List<DonViViewModel> lst_DonVis { get; set; }
        public Guid User_Id { get; set; }
    }
    public class User_PhanQuyen
    {
        public Guid DonVi_Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsFull { get; set; }
    }
    public class MenuInfo
    {
        public Guid Id { get; set; }
        public string STT { get; set; }
        public string TenMenu { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public int ThuTu { get; set; }
        public Guid? Parent_Id { get; set; }
        public List<MenuInfo> children { set; get; }
        public bool IsUsed { get; set; }
        public bool IsRemove { get; set; }
        public Guid? PhanMem_Id { get; set; }
        public Guid? DonVi_Id { get; set; }
        public Guid? TapDoan_Id { get; set; }
        public Guid? PhongBan_Id { get; set; }
    }
    public class Permission
    {
        public bool View { get; set; }
        public bool Add { get; set; }
        public bool Edit { get; set; }
        public bool Del { get; set; }
        public bool Print { get; set; }
        public bool Cof { get; set; }
    }
    public class MenuView
    {
        public Guid Id { get; set; }
        public string STT { get; set; }
        public string TenMenu { get; set; }
        public string Url { get; set; }
        public Guid? Parent_Id { get; set; }
        public int ThuTu { get; set; }
        public string Icon { get; set; }
        public List<MenuView> children { set; get; }
        public Permission permission { set; get; }
        public Guid? PhanMem_Id { get; set; }
        public Guid? DonVi_Id { get; set; }
        public Guid? TapDoan_Id { get; set; }
        public Guid? PhongBan_Id { get; set; }
    }
    public class ApplicationUserListViewModel
    {
        public Guid Id { get; set; }

        public List<IdentityUserRole<string>> Roles { get; set; }
    }
    public class DonViViewModel
    {
        public Guid? Id { get; set; }
        public string MaDonVi { get; set; }
        public string TenDonVi { get; set; }
        public string STT { get; set; }
        public bool IsUsed { set; get; }
        public bool IsRemove { set; get; }
        public Guid? Parent_Id { set; get; }
        public int ThuTu { set; get; }
        public bool Checked { set; get; }
        public bool HasChild { set; get; }
        public bool IsFull { get; set; }
        public bool IsLeaf { get; set; }
        public int Level { get; set; }
        public List<DonViViewModel> children { set; get; }
        public List<User_PhanQuyen> lst_user_phanquyen { set; get; }
    }

    public class ClassListKho
    {
        public Guid Id { get; set; }
        public string MaKho { get; set; }
        public string TenKho { get; set; }
        public string TenDonVi { get; set; }
        public string MaThietBi { get; set; }
        public string TenThietBi { get; set; }
        public string Cauhinh { get; set; }
        public string SoSeri { get; set; }
        public string ModelThietBi { get; set; }
        public string DonViTinh { get; set; }
        public string TinhTrangThietBi { get; set; }
        public int SoLuong { get; set; }
    }
    public class ImportDonVi
    {
        public IFormFile file { get; set; }
        public Guid? Parent_Id { get; set; }
    }


    public class FirebaseConfig
    {
        public string apiKey { get; set; }
        public string authDomain { get; set; }
        public string projectId { get; set; }
        public string storageBucket { get; set; }
        public string messagingSenderId { get; set; }
        public string appId { get; set; }
        public string measurementId { get; set; }
        public string RegistrationToken { get; set; }
    }

    public class ClassChucVu
    {
        public Guid Id { get; set; }
        public string MaChucVu { get; set; }
        public string TenChucVu { get; set; }
    }

    public class ClassBoPhan
    {
        public Guid Id { get; set; }
        public string MaBoPhan { get; set; }
        public string TenBoPhan { get; set; }
        public Guid? PhongBan_Id { get; set; }
        public Guid? BoPhan_Id { get; set; }
    }
    public class ClassPhongBan
    {
        public Guid Id { get; set; }
        public string MaPhongBan { get; set; }
        public string TenPhongBan { get; set; }
        public Guid? DonVi_Id { get; set; }
        public Guid? PhongBan_Id { get; set; }
    }
    public class ClassDonVi
    {
        public Guid Id { get; set; }
        public string MaDonVi { get; set; }
        public string TenDonVi { get; set; }
        public Guid? TapDoan_Id { get; set; }
        public Guid? DonVi_Id { get; set; }
    }
    public class ClassDoiTac
    {
        public Guid Id { get; set; }
        public string MaDoiTac { get; set; }
        public string TenDoiTac { get; set; }
        public Guid LoaiDoiTac_Id { get; set; }
        public Guid NhomDoiTac_Id { get; set; }
        public string MaSoThue { get; set; }
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string NguoiLienHe { get; set; }
    }
    public class ClassTapDoan
    {
        public Guid Id { get; set; }
        public string MaTapDoan { get; set; }
        public string TenTapDoan { get; set; }
        public Guid? TapDoan_Id { get; set; }
    }

    public class ClassLoaiKhachHang
    {
        public Guid Id { get; set; }
        public string MaLoaiKhachHang { get; set; }
        public string TenLoaiKhachHang { get; set; }
        public Guid? LoaiKhachHang_Id { get; set; }
    }

    public class ClassKhachHang
    {
        public Guid Id { get; set; }
        public string MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public Guid LoaiKhachHang_Id { get; set; }
        public Guid? DonViCungCap_Id { get; set; }
        public string DiaChi { get; set; }
        public string SoDienThoai { get; set; }
        public string MaSoThue { get; set; }
        public string Email { get; set; }
        public string NguoiLienHe { get; set; }
        public string Fax { get; set; }
    }

    public class ClassXuongSanXuat
    {
        public Guid Id { get; set; }
        public string MaXuongSanXuat { get; set; }
        public string TenXuongSanXuat { get; set; }
        public string NoiDat { get; set; }
        public string Type { get; set; }

    }

    public class ClassCauTrucKho
    {
        public Guid Id { get; set; }
        public string MaCauTrucKho { get; set; }
        public string TenCauTrucKho { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Số không được âm.")]
        public int SucChua { get; set; }
        public int ThuTu { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Số không được âm.")]
        public int ViTri { get; set; }
        public Guid? CauTrucKho_Id { get; set; }
        public Guid? PhongBan_Id { get; set; }
        public string QRCode { get; set; }
    }

    public class ClassKe
    {
        public Guid Id { get; set; }
        public string MaKe { get; set; }
        public string TenKe { get; set; }
        public Guid Kho_Id { get; set; }
        public string Barcode { get; set; }
    }
    public class ClassTang
    {
        public Guid Id { get; set; }
        public string MaTang { get; set; }
        public string TenTang { get; set; }
        public Guid Ke_Id { get; set; }
    }
    public class ClassNgan
    {
        public Guid Id { get; set; }
        public string MaNgan { get; set; }
        public string TenNgan { get; set; }
        public Guid Tang_Id { get; set; }
        public int SoLuongToiDa { get; set; }
    }
    public class ClassNhomVatTu
    {
        public Guid Id { get; set; }
        public string MaNhomVatTu { get; set; }
        public string TenNhomVatTu { get; set; }
        public Guid? NhomVatTu_Id { get; set; }
    }
    public class ClassVatTu
    {
        public Guid Id { get; set; }
        public string MaVatTu { get; set; }
        public string TenVatTu { get; set; }
        public Guid NhomVatTu_Id { get; set; }
        public string QuyCach { get; set; }
        public string KichThuoc { get; set; }
        public Guid? MauSac_Id { get; set; }
        public Guid DonViTinh_Id { get; set; }
        public string DonViQuyDoi { get; set; }
        public string TiLeQuyDoi { get; set; }
        public string Barcode { get; set; }
    }
    public class ClassLoaiDinhMucTonKho
    {
        public Guid Id { get; set; }
        public string MaLoaiDinhMucTonKho { get; set; }
        public string TenLoaiDinhMucTonKho { get; set; }
    }
    public class ClassThietBi
    {
        public Guid Id { get; set; }
        public string MaThietBi { get; set; }
        public string TenThietBi { get; set; }
    }
    public class ClassLoaiSanPham
    {
        public Guid Id { get; set; }
        public string MaLoaiSanPham { get; set; }
        public string TenLoaiSanPham { get; set; }
        public Guid? LoaiSanPham_Id { get; set; }
    }
    public class ClassSanPham
    {
        public Guid Id { get; set; }
        public string MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public Guid LoaiXe_Id { get; set; }
        public string KichThuoc { get; set; }
        public Guid DonViTinh_Id { get; set; }
        public List<ClassMauSacSanPham> chiTietMauSacs { get; set; }
    }
    public class ClassMauSacSanPham
    {
        public Guid Id { get; set; }
        public Guid? SanPham_Id { get; set; }
        public Guid MauSac_Id { get; set; }
    }

    public class ClassMauSac
    {
        public Guid Id { get; set; }
        public string MaMauSac { get; set; }
        public string TenMauSac { get; set; }
        public string GiaTriKhac { get; set; }
        public bool IsSuDung { get; set; }
    }
    //for admin
    public class ClassAdminActive
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
        public Guid Role_Id { get; set; }
        public Guid? Roleold_Id { get; set; }
    }
    public class ClassAddDonViCBNV
    {
        public Guid Id { get; set; }
        public Guid User_Id { get; set; }
        public Guid TapDoan_Id { get; set; }
        public Guid DonVi_Id { get; set; }
        public Guid PhongBan_Id { get; set; }
        public Guid? BoPhan_Id { get; set; }
        public Guid? ChucVu_Id { get; set; }
    }
    //for cnnv
    public class ClassCBNVActive
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }
        public Guid? PhanMem_Id { get; set; }
        public Guid? DonVi_Id { get; set; }
        public List<ClassRoleCBNV> chiTietRoles { get; set; }
        public List<ClassRoleCBNVOld> chiTietRolesOld { get; set; }
    }
    public class ClassRoleCBNV
    {
        public Guid Role_Id { get; set; }
    }
    public class ClassRoleCBNVOld
    {
        public Guid? Roleold_Id { get; set; }
    }
    public class ClassNLSX
    {
        public Guid? ThietBi_Id { get; set; }
        public Guid? SanPham_Id { get; set; }
        public List<ClassChiTietNLSX> chiTietNLSXs { get; set; }
    }

    public class ClassChiTietNLSX
    {
        public string ThoiGian { get; set; }
        public int SLNhanSu { get; set; }
        public string NangXuat { get; set; }
        public int SoLuongSanPham { get; set; }
        public string GhiChu { get; set; }
    }

    public class ClassLoaiKeHoach
    {
        public Guid Id { get; set; }
        public string MaLoaiKeHoach { get; set; }
        public string TenLoaiKeHoach { get; set; }
    }

    public class ClassLoaiNhaCungCap
    {
        public Guid Id { get; set; }
        public string MaLoaiNhaCungCap { get; set; }
        public string TenLoaiNhaCungCap { get; set; }
    }

    public class Classlkn_ThongTinVatTu
    {
        public Guid Id { get; set; }
        public Guid? VatTu_Id { get; set; }
        public Guid? NhaCungCap_Id { get; set; }
        public string NgayNhapVT_SP { get; set; }
        public string ThoiGianSuDung { get; set; }
        [StringLength(250)]
        public string GhiChu { get; set; }
    }

    public class Classlkn_PhieuDeNghiMuaHang
    {
        public Guid Id { get; set; }
        public string MaPhieuYeuCau { get; set; }
        public Guid UserYeuCau_Id { get; set; }
        public Guid UserKeToan_Id { get; set; }
        public Guid UserKiemTra_Id { get; set; }
        public Guid UserDuyet_Id { get; set; }
        public string NgayHoanThanhDukien { get; set; }
        public string NgayYeuCau { get; set; }
        public string ThoiGianSuDung { get; set; }
        public bool IsCKD { get; set; }
        public List<Classlkn_ChiTietPhieuDeNghiMuaHang> chiTiet_phieumuahangs { get; set; }

    }
    public class Classlkn_ChiTietPhieuDeNghiMuaHang
    {
        public Guid lkn_PhieuMuaHang_Id { get; set; }
        public Guid? VatTu_Id { get; set; }
        public Guid? SanPham_Id { get; set; }
        public Guid? Bom_Id { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm.")]
        public int SoLuong { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm.")]
        public int SoLuongTonKho { get; set; }
        [StringLength(250)]
        public string GhiChu { get; set; }
        [StringLength(250)]
        public string HangMucSuDung { get; set; }
    }

    public class ClassXacNhanPhieuDeNghi
    {
        //xác nhận phiếu đề nghị
        public Guid Id { get; set; }
        public bool isXacNhan { get; set; }
        [StringLength(250)]
        public string LyDoTuChoi { get; set; }
        public List<Classlkn_ChiTietPhieuDeNghiMuaHang> chiTiet_phieumuahangs { get; set; }
    }

    public class ClassTaiFilePhieuDeNghi
    {
        //xác nhận phiếu đề nghị
        public Guid Id { get; set; }
        [StringLength(250)]
        public string File { get; set; }

    }
    public class ClassNgayXacNhanHangVe
    {
        public Guid Id { get; set; }
        public string NgayXacNhanHangVe { get; set; }
        public Guid? UserThuMua_Id { get; set; }

    }

    public class Classlkn_PhieuDatHangNoiBo
    {
        public Guid Id { get; set; }
        public string MaDonHang { get; set; }
        public Guid UserYeuCau_Id { get; set; }
        public Guid UserNhan_Id { get; set; }
        public Guid UserThuMua_Id { get; set; }
        public Guid UserKeToan_Id { get; set; }
        public Guid UserKiemTra_Id { get; set; }
        public Guid UserDuyet_Id { get; set; }
        public string NgayHoanThanhDukien { get; set; }
        public string NgayYeuCau { get; set; }
        [StringLength(250)]
        public string YeuCau { get; set; }
        [StringLength(250)]
        public string DiaDiemGiaoHang { get; set; }
        [StringLength(50)]
        public string NguoiNhan_DHNB { get; set; }
        [StringLength(50)]
        public string EmailNguoiNhan_DHNB { get; set; }
        public List<Classlkn_ChiTietDatHangNoiBo> chiTiet_DatHangNoiBos { get; set; }

    }

    public class Classlkn_ChiTietDatHangNoiBo
    {
        public Guid lkn_PhieuMuaHang_Id { get; set; }
        public Guid VatTu_Id { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm.")]
        public int SoLuong { get; set; }
        [StringLength(250)]
        public string GhiChu { get; set; }
        [StringLength(250)]
        public string HangMucSuDung { get; set; }
    }

    public class ClassXacNhanDatHangNoiNo
    {
        //xác nhận phiếu đề nghị
        public Guid Id { get; set; }
        public bool isXacNhan { get; set; }
        [StringLength(250)]
        public string LyDoTuChoi { get; set; }
        public List<Classlkn_ChiTietPhieuDeNghiMuaHang> chiTiet_phieumuahangs { get; set; }
    }

    public class Classlkn_PhieuTraHangNCC
    {
        public Guid Id { get; set; }
        public string MaPhieuTraHang { get; set; }
        public Guid UserLap_Id { get; set; }
        public Guid BenNhan_Id { get; set; }
        public Guid BenVanChuyen_Id { get; set; }
        public Guid UserDuyet_Id { get; set; }
        public string NgayYeuCau { get; set; }
        public string FileDanhGiaChatLuong { get; set; }
        public List<Classlkn_ChiTietPhieuTraHangNCC> chiTiet_traHangNCCs { get; set; }

    }
    public class Classlkn_ChiTietPhieuTraHangNCC
    {
        public Guid lkn_PhieuTraHangNCC_Id { get; set; }
        public Guid CauTrucKho_Id { get; set; }
        public Guid? Ke_Id { get; set; }
        public Guid? Ngan_Id { get; set; }
        public Guid? Tang_Id { get; set; }
        public Guid VatTu_Id { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm.")]
        public int SoLuongTraNCC { get; set; }
        public string SoPO { get; set; }
        [StringLength(250)]
        public string GhiChu { get; set; }
    }
    public class ClassXacNhanTraHangNCC
    {
        public Guid Id { get; set; }
        public bool isXacNhan { get; set; }
        [StringLength(250)]
        public string LyDoTuChoi { get; set; }
    }

    public class Classlkn_PhieuNhanHang
    {
        public Guid Id { get; set; }
        public string MaPhieuNhanHang { get; set; }
        public Guid? PhieuMuaHang_Id { get; set; }
        public string NgayHangVe { get; set; }
        public string FileDinhKem { get; set; }
        public Guid? UserThuMua_Id { get; set; }
        public Guid UserYeuCau_Id { get; set; }
        public List<Classlkn_ChiTietPhieuNhanHang> chiTiet_PhieuNhanHang { get; set; }

    }
    public class Classlkn_ChiTietPhieuNhanHang
    {
        public Guid lkn_PhieuNhanHang_Id { get; set; }
        public Guid? VatTu_Id { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm.")]
        public int SoLuongNhan { get; set; }
        [StringLength(250)]
        public string GhiChu { get; set; }
        [StringLength(250)]
        public string HangMucSuDung { get; set; }
    }


    public class Classlkn_PhieuNhapKhoVatTu
    {
        public Guid Id { get; set; }
        public string MaPhieuNhapKhoVatTu { get; set; }
        public Guid? PhieuNhanHang_Id { get; set; }
        public string NgayNhan { get; set; }
        [StringLength(250)]
        public string SoHoaDon { get; set; }
        public string NgayHoaDon { get; set; }
        public Guid? CauTrucKho_Id { get; set; }
        public Guid UserNhan_Id { get; set; }
        public Guid NhaCungCap_Id { get; set; }
        [StringLength(250)]
        public string NoiDungNhanVatTu { get; set; }
        public Guid? UserPhuTrach_Id { get; set; }
        public Guid? UserThongKe_Id { get; set; }
        public Guid? UserQC_Id { get; set; }
        public List<Classlkn_ChiTietPhieuNhapKhoVatTu> chiTiet_PhieuNhapKhoVatTus { get; set; }

    }
    public class Classlkn_ChiTietPhieuNhapKhoVatTu
    {
        public Guid lkn_PhieuNhapKhoVatTu_Id { get; set; }
        public Guid? Ke_Id { get; set; }
        public Guid? Tang_Id { get; set; }
        public Guid? Ngan_Id { get; set; }
        public Guid? VatTu_Id { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm.")]
        public int SoLuongNhap { get; set; }
        [StringLength(250)]
        public string GhiChu { get; set; }
        public string ThoiGianSuDung { get; set; }
    }

    public class Classlkn_PhieuNhapKhoCKD
    {
        public Guid Id { get; set; }
        public string MaPhieuNhapKhoCKD { get; set; }
        public string NgayNhan { get; set; }
        public Guid? CauTrucKho_Id { get; set; }
        public Guid? Lot_Id { get; set; }
        public Guid UserNhan_Id { get; set; }
        public List<Classlkn_ChiTietPhieuNhapKhoCKD> chiTiet_PhieuNhapKhoCKDs { get; set; }

    }
    public class Classlkn_ChiTietPhieuNhapKhoCKD
    {
        public Guid lkn_PhieuNhapKhoVatTu_Id { get; set; }
        public Guid? VatTu_Id { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm.")]
        public int SoLuongNhap { get; set; }
        [StringLength(250)]
        public string GhiChu { get; set; }
    }
    public class Classlkn_PhieuNhapKhoCKDMobile
    {
        public string NgayNhan { get; set; }
        public string QRCode { get; set; }
        public Guid? CauTrucKho_Id { get; set; }
        public Guid UserNhan_Id { get; set; }
        public string SoLot { get; set; }
        public List<Classlkn_ChiTietPhieuNhapKhoCKDMobile> chiTiet_PhieuNhapKhoCKDs { get; set; }

    }
    public class Classlkn_ChiTietPhieuNhapKhoCKDMobile
    {
        public string MaVatTu { get; set; }
        public Guid? VatTu_Id { get; set; }
        public int SoLuongTrenKe { get; set; }
    }
    public class Classlkn_ViTriLuuKho
    {
        public Guid Id { get; set; }
        public Guid VatTu_Id { get; set; }
        public Guid? Ke_Id { get; set; }
        public Guid? Tang_Id { get; set; }
        public Guid? Ngan_Id { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm.")]
        public int SoLuong { get; set; }

    }

    public class Classlkn_ViTriLuuKhoThanhPham
    {
        public Guid Id { get; set; }
        public Guid SanPham_Id { get; set; }
        public Guid MauSac_Id { get; set; }
        public Guid? Ke_Id { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm.")]
        public int SoLuong { get; set; }

    }




    public class Classlkn_PhieuDeNghiCapVatTu
    {
        public Guid Id { get; set; }
        public bool IsLoaiPhieu { get; set; }
        public Guid? UserLapPhieu_Id { get; set; }
        public Guid? UserKhoVatTu_Id { get; set; }
        public Guid? UserDuyet_Id { get; set; }
        public Guid? UserKiemTra_Id { get; set; }
        public Guid XuongSanXuat_Id { get; set; }
        public Guid? SanPham_Id { get; set; }
        public string NgayYeuCau { get; set; }
        public string NgaySanXuat { get; set; }
        public List<Classlkn_ChiTietPhieuDeNghiCapVatTu> chiTiet_PhieuDeNghiCapVatTus { get; set; }

    }
    public class Classlkn_ChiTietPhieuDeNghiCapVatTu
    {
        public Guid? lkn_DinhMucVatTu_Id { get; set; }
        public Guid VatTu_Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        [Range(0, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0.")]
        public double SoLuong { get; set; }
        public int? SoLuongKH { get; set; }
        public string HanMucSuDung { get; set; }
        [StringLength(250)]
        public string GhiChu { get; set; }
    }

    public class ClassXacNhanPhieuDeNghiCapVatTu
    {
        //xác nhận phiếu đề nghị
        public Guid Id { get; set; }
        public bool isXacNhan { get; set; }
        [StringLength(250)]
        public string LyDoTuChoi { get; set; }

    }
    public class ClassXacNhanPhieuXuatKhoVatTu
    {
        //xác nhận phiếu 
        public Guid Id { get; set; }
        public bool isXacNhan { get; set; }
        [StringLength(250)]
        public string LyDoTuChoi { get; set; }

    }
    public class ClassChiTiet
    {
        public Guid Id { get; set; }
        [StringLength(50)]
        public string MaChiTiet { get; set; }
        [StringLength(250)]
        public string TenChiTiet { get; set; }
        public Guid SanPham_Id { get; set; }
        public Guid DonViTinh_Id { get; set; }
        [StringLength(50)]
        public string KichThuoc { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm.")]
        public int SoLuongChiTiet { get; set; }
    }
    public class ClassChiTiet_QuyTrinhSX
    {
        public Guid Id { get; set; }
        public Guid lkn_ChiTiet_Id { get; set; }
        public Guid lkn_QuyTrinhSX_Id { get; set; }
    }

    public class Classlkn_PhieuDieuChuyen
    {
        public Guid Id { get; set; }
        public string MaPhieuDieuChuyen_ThanhLy { get; set; }
        public Guid UserLap_Id { get; set; }
        public Guid KhoDi_Id { get; set; }
        public Guid? KhoDen_Id { get; set; }
        public string NgayYeuCau { get; set; }
        public List<Classlkn_ChiTietPhieuDieuChuyen> chiTiet_PhieuDieuChuyens { get; set; }

    }
    public class Classlkn_ChiTietPhieuDieuChuyen
    {
        public Guid lkn_PhieuDieuChuyen_ThanhLy_Id { get; set; }
        public Guid VatTu_Id { get; set; }
        public Guid? MauSac_Id { get; set; }
        public Guid CauTrucKho_Id { get; set; }
        public Guid lkn_ChiTietKhoBegin_Id { get; set; }
        public int SoLuongDieuChuyen { get; set; }
        [StringLength(250)]
        public string GhiChu { get; set; }
    }
    public class Classlkn_PutPhieuDieuChuyen
    {
        public Guid Id { get; set; }
        public List<Classlkn_PutChiTietPhieuDieuChuyen> chiTiet_PhieuDieuChuyens { get; set; }

    }
    public class Classlkn_PutChiTietPhieuDieuChuyen
    {
        public Guid VatTu_Id { get; set; }
        public Guid CauTrucKho_Id { get; set; }
        public Guid? MauSac_Id { get; set; }
        public Guid lkn_ChiTietKhoBegin_Id { get; set; }
        public int SoLuongDieuChuyen { get; set; }
        [StringLength(250)]
        public string GhiChu { get; set; }
    }
    public class Classlkn_PhieuThanhLy
    {
        public Guid Id { get; set; }
        public string MaPhieuDieuChuyen_ThanhLy { get; set; }
        public Guid UserLap_Id { get; set; }
        public Guid KhoThanhLy_Id { get; set; }
        public string NgayYeuCau { get; set; }
        public List<Classlkn_ChiTietPhieuThanhLy> chiTiet_PhieuThanhLys { get; set; }

    }
    public class Classlkn_ChiTietPhieuThanhLy
    {
        public Guid lkn_PhieuDieuChuyen_ThanhLy_Id { get; set; }
        [ForeignKey("VatTu")]
        public Guid VatTu_Id { get; set; }
        public Guid CauTrucKho_Id { get; set; }
        public Guid lkn_ChiTietKhoBegin_Id { get; set; }
        public int SoLuongThanhLy { get; set; }
        [StringLength(250)]
        public string GhiChu { get; set; }
    }
    public class Classlkn_PutPhieuThanhLy
    {
        public Guid Id { get; set; }
        public List<Classlkn_PutChiTietPhieuThanhLy> chiTiet_PhieuThanhLys { get; set; }

    }
    public class Classlkn_PutChiTietPhieuThanhLy
    {
        public Guid lkn_PhieuDieuChuyen_ThanhLy_Id { get; set; }
        [ForeignKey("VatTu")]
        public Guid VatTu_Id { get; set; }
        public Guid CauTrucKho_Id { get; set; }
        public Guid lkn_ChiTietKhoBegin_Id { get; set; }
        public int SoLuongThanhLy { get; set; }
        [StringLength(250)]
        public string GhiChu { get; set; }
    }
    public class Classlkn_DinhMucTonKho
    {
        public Guid Id { get; set; }
        public string MaDinhMuc { get; set; }
        public Guid? CauTrucKho_Id { get; set; }
        public string NgayNhap { get; set; }
        public Guid UserLap_Id { get; set; }
        public Guid? VatTu_Id { get; set; }
        public int SLTonKhoToiThieu { get; set; }
        public int SLTonKhoToiDa { get; set; }
        public Guid LoaiDinhMucTonKho_Id { get; set; }
    }

    public class Classlkn_PhieuXuatKhoVatTu
    {
        public Guid Id { get; set; }
        [StringLength(50)]
        public string MaPhieuXuatKhoVatTu { get; set; }
        public Guid UserLapPhieu_Id { get; set; }
        public Guid? UserDuyet_Id { get; set; }
        public Guid? UserPhuTrachBoPhan_Id { get; set; }
        public Guid? UserNhan_Id { get; set; }
        public Guid PhieuDeNghiCapVatTu_Id { get; set; }
        public Guid Kho_Id { get; set; }
        public string NgayYeuCau { get; set; }
        [StringLength(250)]
        public string LyDoXuat { get; set; }
        public bool IsXacNhan { get; set; } = false;
        public bool IsPhuTrachBoPhanXacNhan { get; set; } = false;
        public bool IsNhanXacNhan { get; set; } = false;
        public bool IsDuyetXacNhan { get; set; } = false;
        [StringLength(250)]
        public string LyDoPhuTrachBoPhanTuChoi { get; set; }
        [StringLength(250)]
        public string LyDoNhanTuChoi { get; set; }
        [StringLength(250)]
        public string LyDoDuyetTuChoi { get; set; }
        public int ThuTu { get; set; }
        public bool IsHoanThanh { get; set; } = false;
        public List<Classlkn_ChiTietPhieuXuatKhoVatTu> chiTiet_ChiTietPhieuXuatKhoVatTus { get; set; }
    }

    public class Classlkn_ChiTietPhieuXuatKhoVatTu
    {
        public Guid VatTu_Id { get; set; }
        public int SoLuong { get; set; }
        public string GhiChu { get; set; }
        public Guid lkn_ChiTietPhieuDeNghiCapVatTu_Id { get; set; }
        public List<Classlkn_ChiTietLuuVatTu> chiTiet_LuuVatTus { get; set; }
    }

    public class Classlkn_ChiTietLuuVatTu
    {
        public Guid lkn_ChiTietKhoVatTu_Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập số lượng thực xuất.")]
        [Range(0, double.MaxValue, ErrorMessage = "Số lượng thực xuất phải lớn hơn hoặc bằng 0.")]
        public double SoLuongThucXuat { get; set; }
    }

    public class Classlkn_PhieuKiemKe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Guid? UserLap_Id { get; set; }
        public Guid? PhongBan_Id { get; set; }
        public Guid? UserDuyet2_Id { get; set; }
        public Guid? UserDuyet3_Id { get; set; }
        public Guid? UserDuyet4_Id { get; set; }
        public string NgayKiemKe { get; set; }
        public List<Classlkn_ChiTietPhieuKiemKe> chiTiet_PhieuKiemKes { get; set; }
    }
    public class Classlkn_ChiTietPhieuKiemKe
    {
        public Guid lkn_PhieuKiemKe_Id { get; set; }
        public Guid? VatTu_Id { get; set; }
        public double SoLuongTrenPhamMem { get; set; }
        public double SoLuongKiemKe { get; set; }
        [StringLength(250)]
        public string GhiChu { get; set; }
    }
    public class ClassXacNhanPhieuKiemKe
    {
        public Guid Id { get; set; }
        public bool? IsXacNhan { get; set; }
        [StringLength(250)]
        public string LyDoTuChoi { get; set; }
    }
    public class ViTriViewModel
    {
        public string STT { get; set; }
        public Guid? ViTri_Id { get; set; }
        public string MaViTri { get; set; }
        public string TenViTri { get; set; }
        public bool IsLoi { get; set; }
        public bool IsTrung { get; set; }
        public List<string> lst_Lois { get; set; }
        public Guid? BaiXe_ID { get; set; }
        public string MaBaiXe { get; set; }
        public string TenBaiXe { get; set; }
    }
    public class ImportViTri
    {
        public IFormFile file { get; set; }
        // public Guid DonVi_Id { get; set; }
    }


}
