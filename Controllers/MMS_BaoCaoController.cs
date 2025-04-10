using ERP.Infrastructure;
using ERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using static ERP.Data.MyDbContext;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Presentation;
using System.IO;
using OfficeOpenXml;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ERP.Helpers;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using System.Threading;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MMS_BaoCaoController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        private readonly DataService _master;
        private readonly MMS_NotificationController thongbao;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        public MMS_BaoCaoController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment, DataService master, MMS_NotificationController _thongbao)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
            _master = master;
            thongbao = _thongbao;
        }


        [HttpGet("LenhHoanThanh")]
        public async Task<ActionResult> GetLenh(string keyword = null)
        {
            var dataList = await _master.GetDiadiem();
            Expression<Func<LichSuBaoDuong, bool>> whereFunc = item => !item.IsDeleted
            && item.PhuongTien_Id != null
            && item.IsLenhHoanThanh && !item.IsHoanThanh
            && (string.IsNullOrEmpty(keyword)
            || item.PhuongTien.BienSo1.Contains(keyword.Trim().ToLower())
            || item.PhuongTien.BienSo2.Contains(keyword.Trim().ToLower()));
            string[] includes = { "BaoDuong", "PhuongTien","LichSuBaoDuong_ChiTiets.HangMuc",
             };
            var data = uow.LichSuBaoDuongs.GetAll(whereFunc, null, includes)
             .Select(x => new
             {
                 x.Id,
                 x.DiaDiem_Id,
                 TenDiaDiem = dataList.Where(t => !x.IsDeleted && t.Id == x.DiaDiem_Id)?.FirstOrDefault()?.TenDiaDiem,
                 Ngay = string.Format("{0:dd/MM/yyyy}", x.Ngay),
                 NgayXacNhan = string.Format("{0:dd/MM/yyyy}", x.NgayXacNhan),
                 NgayDiBaoDuong = string.Format("{0:dd/MM/yyyy}", x.NgayDiBaoDuong),
                 x.BaoDuong_Id,
                 x.BaoDuong?.TanSuat,
                 x.BaoDuong?.GiaTri,
                 LoaiBaoDuong = x.BaoDuong?.Name,
                 x.PhuongTien_Id,
                 x.PhuongTien?.BienSo1,
                 x.PhuongTien?.SoKM,
                 x.PhuongTien?.SoChuyenXe,
                 ChiSo = x.BaoDuong?.GiaTri,
                 x.NoiDung,
                 x.KetQua,
                 x.ChiPhi,
                 lichSu = x.LichSuBaoDuong_ChiTiets.Where(a => !a.IsDeleted && a.LichSuBaoDuong_Id == x.Id
                ).Select(x => new
                {
                    x.Id,
                    x.LichSuBaoDuong_Id,
                    x.HangMuc?.NoiDungBaoDuong,
                    x.HangMuc?.DinhMuc,
                    x.HangMuc?.LoaiBaoDuong,
                    x.HangMuc?.GhiChu,
                    x.LichSuBaoDuong?.PhuongTien?.BienSo1,
                    x.LichSuBaoDuong?.PhuongTien?.BienSo2,
                }),
             });
            return Ok(data);
        }


        [HttpGet("LichSuBaoDuongChiTietTheoPT")]
        public ActionResult Get(Guid? BaoDuong_Id)
        {
            Expression<Func<LichSuBaoDuong_ChiTiet, bool>> whereFunc = item => !item.IsDeleted
            && item.PhuongTien_Id != null
            && item.LichSuBaoDuong_Id == BaoDuong_Id;
            string[] includes = { "PhuongTien", "LichSuBaoDuong", "HangMuc" };
            var data = uow.LichSuBaoDuong_ChiTiets.GetAll(whereFunc, x => x.OrderByDescending(x => x.CreatedDate), includes)
             .Select(x => new
             {
                 x.Id,
                 x.PhuongTien_Id,
                 Ngay = string.Format("{0:dd/MM/yyyy HH:mm}", x.CreatedDate),
                 x.CreatedDate,
                 x.LichSuBaoDuong_Id,
                 x.HangMuc?.NoiDungBaoDuong,
                 x.LichSuBaoDuong?.DiaDiem_Id,
                 x.HangMuc_Id,
                 DinhMuc = string.Format("{0:N0}", x?.HangMuc?.DinhMuc),
                 DinhMuc2 = x.HangMuc?.DinhMuc.ToString(),
                 x.HangMuc?.LoaiBaoDuong,
                 x.HangMuc?.GhiChu,
                 x.PhuongTien?.BienSo1,
                 ChiPhi = string.Format("{0:N0}", x?.ChiPhi),
                 ChiPhi2 = x?.ChiPhi,
                 GhiChuBD = x.LichSuBaoDuong?.GhiChu,
                 SoKM2 = x.LichSuBaoDuong?.SoKM,
                 SoKM = string.Format("{0:N0}", x.PhuongTien?.SoKM),
             });
            return Ok(data);
        }

        [HttpGet("LichSuBaoDuongChiTiet")]
        public async Task<ActionResult> Chitiet(Guid? PhuongTien_Id, Guid? HangMuc_Id, string keyword = null)
        {
            Expression<Func<LichSuBaoDuong_ChiTiet, bool>> whereFunc = item => !item.IsDeleted
            && item.PhuongTien_Id != null
            && item.PhuongTien_Id == PhuongTien_Id
            && (HangMuc_Id == null || item.HangMuc_Id == HangMuc_Id)
            && (string.IsNullOrEmpty(keyword) || item.PhuongTien.BienSo1.ToLower().Replace("-", "").Replace(".", "").Contains(keyword.ToLower().Replace("-", "").Replace(".", ""))
            || item.HangMuc.NoiDungBaoDuong.ToLower().Contains(keyword.ToLower()));
            string[] includes = { "PhuongTien", "HangMuc", "LichSuBaoDuong" };
            var data = uow.LichSuBaoDuong_ChiTiets.GetAll(whereFunc, x => x.OrderByDescending(x => x.CreatedDate), includes)
             .Select(x => new
             {
                 x.Id,
                 x.PhuongTien?.BienSo1,
                 x.PhuongTien?.BienSo2,
                 Ngay = string.Format("{0:dd/MM/yyyy HH:mm}", x.CreatedDate),
                 x.CreatedDate,
                 x.LichSuBaoDuong_Id,
                 x.HangMuc?.NoiDungBaoDuong,
                 x.HangMuc?.LoaiBaoDuong,
                 SoKM = string.Format("{0:N0}", x.LichSuBaoDuong.SoKM),
                 //x.ChiPhi,
                 DinhMuc = string.Format("{0:N0}", x.HangMuc?.DinhMuc),
                 ChiPhi = string.Format("{0:N0}", x.ChiPhi),
                 ChiPhi2 = string.Format("{0:N0}", x.ChiPhi),
                 x.HangMuc?.GhiChu,
                 //x.LichSuBaoDuong.NgayHoanThanh,
             });
            return Ok(data);
        }

        [HttpGet("LichSuBaoDuongTheoPT")]
        public async Task<ActionResult> GetLichSuBaoDuongBienSo(Guid? PhuongTien_Id, string keyword = null)
        {
            var dataList = await _master.GetDiadiem();
            Expression<Func<LichSuBaoDuong, bool>> whereFunc = item => !item.IsDeleted
            && item.PhuongTien_Id != null
            && item.PhuongTien_Id == PhuongTien_Id
              && (string.IsNullOrEmpty(keyword) || item.PhuongTien.BienSo1.ToLower().Replace("-", "").Replace(".", "").Contains(keyword.ToLower().Replace("-", "").Replace(".", "")));
            string[] includes = { "PhuongTien", "PhuongTien.DM_TinhTrang", "LichSuBaoDuong_ChiTiets.HangMuc" };
            var data = uow.LichSuBaoDuongs.GetAll(whereFunc, x => x.OrderByDescending(x => x.CreatedDate), includes)
             .Select(x => new
             {
                 x.Id,
                 x.DiaDiem_Id,
                 x.PhuongTien?.BienSo1,
                 TenDiaDiem = dataList.Where(t => !x.IsDeleted && t.Id == x.DiaDiem_Id)?.FirstOrDefault()?.TenDiaDiem,
                 Ngay = string.Format("{0:dd/MM/yyyy HH:mm}", x.Ngay),
                 NgayXacNhan = string.Format("{0:dd/MM/yyyy HH:mm}", x.NgayXacNhan),
                 NgayDiBaoDuong = string.Format("{0:dd/MM/yyyy HH:mm}", x.NgayDiBaoDuong),
                 NgayDeXuatHoanThanh = string.Format("{0:dd/MM/yyyy HH:mm}", x.NgayDeXuatHoanThanh),
                 NgayHoanThanh = string.Format("{0:dd/MM/yyyy HH:mm} ", x.NgayHoanThanh),
                 x.BaoDuong_Id,
                 x.NguoiYeuCau,
                 x.NguoiXacNhan,
                 x.NguoiDiBaoDuong,
                 x.NguoiDeXuatHoanThanh,
                 x.NguoiXacNhanHoanThanh,
                 x.NoiDung,
                 KetQua = x.GhiChu,
                 x.GhiChu,
                 SoKM = x.SoKM.ToString(),
                 x.IsYeuCau,
                 x.IsDuyet,
                 x.IsBaoDuong,
                 x.IsLenhHoanThanh,
                 x.HinhAnh,
                 ChiPhi = x?.ChiPhi > 0 ? string.Format("{0:N0}", x.ChiPhi) : "",
                 ChiPhi_TD = x?.ChiPhi_TD > 0 ? string.Format("{0:N0}", x.ChiPhi_TD) : "",
                 TongChiPhi = x.IsHoanThanh ?
                     (x.ChiPhi_TD > 0 ? string.Format("{0:N0}", x.ChiPhi_TD) : "") :
                     (x.ChiPhi > 0 ? string.Format("{0:N0}", x.ChiPhi) : ""),
                 x.IsHoanThanh,
                 x.PhuongTien_Id,
                 //  TinhTrang = x.PhuongTien?.DM_TinhTrang?.Name,
                 TinhTrang = x.IsHoanThanh ? "Đã hoàn thành bảo dưỡng" :
                 (x.IsBaoDuong && x.IsYeuCau && x.IsDuyet && x.IsLenhHoanThanh && !x.IsHoanThanh) ? "Đã được đề xuất hoàn thành bảo dưỡng" :
                    (x.IsBaoDuong && x.IsYeuCau && x.IsDuyet && !x.IsLenhHoanThanh && !x.IsHoanThanh) ? "Đang bảo dưỡng" :
                     (x.IsYeuCau && !x.IsBaoDuong && !x.IsDuyet && !x.IsHoanThanh) ? "Đã được yêu cầu bảo dưỡng" :
                     (x.IsDuyet && x.IsYeuCau && !x.IsBaoDuong && !x.IsHoanThanh) ? "Đã được duyệt bảo dưỡng" :
                     "Đã đến hạn bảo dưỡng",

                 lichSu = x.LichSuBaoDuong_ChiTiets.Where(a => !a.IsDeleted && a.LichSuBaoDuong_Id == x.Id
                ).Select(x => new
                {
                    x.Id,
                    Ngay = string.Format("{0:dd/MM/yyyy HH:mm}", x.CreatedDate),
                    x.CreatedDate,
                    x.LichSuBaoDuong_Id,
                    x.HangMuc?.NoiDungBaoDuong,
                    DinhMuc = x.HangMuc?.DinhMuc.ToString(),
                    x.HangMuc_Id,
                    DinhMuc2 = x.HangMuc?.DinhMuc.ToString(),
                    x.HangMuc?.LoaiBaoDuong,
                    x.HangMuc?.GhiChu,
                    SoKM = string.Format("{0:N0}", x.LichSuBaoDuong?.SoKM),
                    ChiPhi = string.Format("{0:N0}", x?.ChiPhi),
                    // x?.ChiPhi.ToString(),
                    x.LichSuBaoDuong?.PhuongTien?.BienSo1,
                    x.LichSuBaoDuong?.PhuongTien?.BienSo2,

                }),
             });
            return Ok(data);
        }

        [HttpGet("LichSuBaoDuong")]
        public async Task<ActionResult> GetLichSuBaoDuong(string keyword = null)
        {
            var dataList = await _master.GetDiadiem();
            Expression<Func<LichSuBaoDuong, bool>> whereFunc = item => !item.IsDeleted
            && item.PhuongTien_Id != null
            && (string.IsNullOrEmpty(keyword) || item.PhuongTien.BienSo1.ToLower().Replace("-", "").Replace(".", "").Contains(keyword.ToLower().Replace("-", "").Replace(".", "")));
            string[] includes = { "BaoDuong", "PhuongTien", "PhuongTien.DM_TinhTrang" };
            var data = uow.LichSuBaoDuongs.GetAll(whereFunc, x => x.OrderByDescending(x => x.IsYeuCau && !x.IsDuyet)
             .ThenByDescending(x => x.CreatedDate), includes)
             .Select(x => new
             {
                 x.Id,
                 x.DiaDiem_Id,
                 Model = x.BaoDuong?.Name,
                 Model_Option = x.BaoDuong?.Option,
                 TenDiaDiem = dataList.Where(t => !x.IsDeleted && t.Id == x.DiaDiem_Id)?.FirstOrDefault()?.TenDiaDiem,
                 Ngay = string.Format("{0:dd/MM/yyyy HH:mm}", x.Ngay),
                 NgayXacNhan = string.Format("{0:dd/MM/yyyy HH:mm}", x.NgayXacNhan),
                 NgayDiBaoDuong = string.Format("{0:dd/MM/yyyy HH:mm}", x.NgayDiBaoDuong),
                 NgayHoanThanh = string.Format("{0:dd/MM/yyyy HH:mm} ", x.NgayHoanThanh),
                 ChiPhi = x?.ChiPhi > 0 ? string.Format("{0:N0}", x.ChiPhi) : "",
                 ChiPhi_TD = x?.ChiPhi_TD > 0 ? string.Format("{0:N0}", x.ChiPhi_TD) : "",
                 x.BaoDuong_Id,
                 x.NguoiYeuCau,
                 x.NguoiXacNhan,
                 x.LyDo,
                 x.IsYeuCau,
                 x.IsDuyet,
                 x.IsBaoDuong,
                 x.IsHoanThanh,
                 x.BaoDuong?.TanSuat,
                 GiaTri = x.BaoDuong?.GiaTri.ToString(),
                 x.PhuongTien_Id,
                 x.PhuongTien?.BienSo1,
                 SoKM_Adsun = x.SoKM.ToString(),
                 SoKM = x.PhuongTien?.SoKM.ToString(),
                 SoChuyenXe = x.PhuongTien?.SoChuyenXe.ToString(),
                 ChiSo = x.BaoDuong?.GiaTri.ToString(),
                 x.NoiDung,
                 x.KetQua,
                 //  TinhTrang = x.IsHoanThanh ? "Đã hoàn thành bảo dưỡng" :
                 //     (x.IsBaoDuong && x.IsYeuCau && x.IsDuyet && !x.IsHoanThanh) ? "Đang bảo dưỡng" :
                 //     (x.IsYeuCau && !x.IsBaoDuong && !x.IsDuyet && !x.IsHoanThanh) ? "Đã được yêu cầu bảo dưỡng" :
                 //     (x.IsDuyet && x.IsYeuCau && !x.IsBaoDuong && !x.IsHoanThanh) ? "Đã được duyệt bảo dưỡng" :
                 //     "Đang hoạt động",
                 TinhTrang = x.PhuongTien?.DM_TinhTrang?.Name,

             });
            return Ok(data);
        }

        [HttpGet("DanhSachYeuCau")]
        public async Task<ActionResult> Get(string keyword = null)
        {
            var dataList = await _master.GetDiadiem();
            Expression<Func<LichSuBaoDuong, bool>> whereFunc = item => !item.IsDeleted
            && item.PhuongTien_Id != null
           && item.TrangThai == null
            && (string.IsNullOrEmpty(keyword) || item.PhuongTien.BienSo1.ToLower().Contains(keyword.ToLower()));
            string[] includes = { "BaoDuong", "PhuongTien" };

            var data = uow.LichSuBaoDuongs.GetAll(whereFunc, x => x.OrderByDescending(x => x.CreatedDate), includes)
             .Select(x => new
             {
                 x.Id,
                 x.DiaDiem_Id,
                 Model = x.BaoDuong?.Name,
                 Model_Option = x.BaoDuong?.Option,
                 TenDiaDiem = dataList.Where(t => !x.IsDeleted && t.Id == x.DiaDiem_Id)?.FirstOrDefault()?.TenDiaDiem,
                 Ngay = string.Format("{0:dd/MM/yyyy HH:mm}", x.Ngay),
                 NgayXacNhan = string.Format("{0:dd/MM/yyyy HH:mm}", x.NgayXacNhan),
                 NgayDiBaoDuong = string.Format("{0:dd/MM/yyyy HH:mm}", x.NgayDiBaoDuong),
                 NgayHoanThanh = string.Format("{0:dd/MM/yyyy HH:mm} ", x.NgayHoanThanh),
                 x.BaoDuong_Id,
                 x.NguoiYeuCau,
                 x.BaoDuong?.TanSuat,
                 GiaTri = x.BaoDuong?.GiaTri.ToString(),
                 x.PhuongTien_Id,
                 x.PhuongTien?.BienSo1,
                 SoKM = x.PhuongTien?.SoKM.ToString(),
                 SoKM_Adsun = x.PhuongTien?.SoKM_Adsun.ToString(),
                 SoChuyenXe = x.PhuongTien?.SoChuyenXe.ToString(),
                 ChiSo = x.BaoDuong?.GiaTri.ToString(),
                 x.NoiDung,
                 x.KetQua,
                 ChiPhi = x.ChiPhi.ToString(),
             });
            return Ok(data);
        }


        [HttpGet("GetLichSuYeuCauBaoDuong_New")]
        public ActionResult GetLichSuYeuCauBaoDuong_New(Guid? User_Id)
        {
            string[] include = { "BaoDuong", "PhuongTien" };
            var query = uow.LichSuBaoDuongs.GetAll(x => !x.IsDeleted
            && x.PhuongTien_Id != null
            && x.TrangThai != null
                && x.CreatedBy == User_Id, x => x.OrderByDescending(p => p.UpdatedDate), include);
            var data = query
            .Select(x => new
            {
                x.Id,
                x?.PhuongTien?.BienSo1,
                Model = x.BaoDuong?.Name,
                Model_Option = x.BaoDuong?.Option,
                NgayYeuCau = string.Format("{0:dd/MM/yyyy HH:mm}", x.Ngay),
                NgayXacNhan = string.Format("{0:dd/MM/yyyy HH:mm}", x.NgayXacNhan),
                SoKM = x.PhuongTien?.SoKM.ToString(),
                SoChuyenXe = x.PhuongTien?.SoChuyenXe.ToString(),
                GiaTri = x.BaoDuong?.GiaTri.ToString(),
                x.NguoiYeuCau,
                x.NguoiXacNhan,
                x.LyDo,
                TrangThai = x.TrangThai == "1" ? "Đã duyệt" : "Đã từ chối"
            }).FirstOrDefault();
            return Ok(data);
        }

        [HttpGet("GetLichSuYeuCauBaoDuongCaNhan")]
        public ActionResult GetLichSuYeuCauBaoDuong(string Ngay, Guid? User_Id, string keyword = null)
        {
            DateTime ngay;
            if (!DateTime.TryParseExact(Ngay, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ngay))
            {
                return StatusCode(StatusCodes.Status409Conflict, "Ngày không đúng định dạng!");
            }
            string[] include = { "BaoDuong", "PhuongTien" };
            var query = uow.LichSuBaoDuongs.GetAll(x => !x.IsDeleted
            && x.PhuongTien_Id != null
                && x.CreatedBy == User_Id
                 && (string.IsNullOrEmpty(keyword) ||
            x.PhuongTien.BienSo1.Contains(keyword.Trim().ToLower())), x => x.OrderByDescending(p => p.CreatedDate), include);
            var data = query
            .Select(x => new
            {
                x.Id,
                x?.PhuongTien?.BienSo1,
                Model = x.BaoDuong?.Name,
                Model_Option = x.BaoDuong?.Option,
                NgayYeuCau = string.Format("{0:dd/MM/yyyy HH:mm}", x.Ngay),
                NgayXacNhan = string.Format("{0:dd/MM/yyyy HH:mm}", x.NgayXacNhan),
                SoKM = x.PhuongTien?.SoKM.ToString(),
                SoChuyenXe = x.PhuongTien?.SoChuyenXe.ToString(),
                GiaTri = x.BaoDuong?.GiaTri.ToString(),
                x.NguoiYeuCau,
                x.NguoiXacNhan,
                x.LyDo,
                TrangThai = x.TrangThai == "1" ? "Đã duyệt" : "Đã từ chối"
            });

            return Ok(data);
        }

        [HttpGet("DSDiBaoDuong")]
        public async Task<ActionResult> GetDS(DateTime TuNgay, DateTime DenNgay, string keyword = null)
        {
            var dataList = await _master.GetDiadiem();
            Expression<Func<LichSuBaoDuong, bool>> whereFunc = item => !item.IsDeleted
            && item.PhuongTien_Id != null
            && item.NgayDiBaoDuong.Value.Date >= TuNgay && item.NgayDiBaoDuong.Value.Date <= DenNgay
            && item.IsBaoDuong && !item.IsLenhHoanThanh
            && (string.IsNullOrEmpty(keyword)
            || item.PhuongTien.BienSo1.Contains(keyword.Trim().ToLower())
            || item.PhuongTien.BienSo2.Contains(keyword.Trim().ToLower()));
            string[] includes = { "BaoDuong", "PhuongTien","LichSuBaoDuong_ChiTiets.HangMuc",
            "PhuongTien.PhuTrachBoPhan","PhuongTien.DM_Loai"
             };
            var data = uow.LichSuBaoDuongs.GetAll(whereFunc, null, includes)
             .Select(x =>
             {
                 var now = DateTime.Now;
                 var ngaySuDung = x.NgayDeXuatHoanThanh != null ? (now - x.NgayDeXuatHoanThanh)?.Days : null;
                 return new
                 {
                     x.Id,
                     x.DiaDiem_Id,
                     x.PhuongTien?.MaPhuongTien,
                     LoaiPT = x.PhuongTien?.DM_Loai?.Name,
                     Model = x.PhuongTien?.DM_Model?.Name,
                     x.PhuongTien?.DM_Model?.Option,
                     NguoiPhuTrach = x.PhuongTien?.PhuTrachBoPhan?.NhanVien,
                     SoNgaySuDung = ngaySuDung,
                     TenDiaDiem = dataList.Where(t => !x.IsDeleted && t.Id == x.DiaDiem_Id)?.FirstOrDefault()?.TenDiaDiem,
                     Ngay = string.Format("{0:dd/MM/yyyy}", x.Ngay),
                     NgayXacNhan = string.Format("{0:dd/MM/yyyy}", x.NgayXacNhan),
                     NgayDiBaoDuong = string.Format("{0:dd/MM/yyyy}", x.NgayDiBaoDuong),
                     x.BaoDuong_Id,
                     x.BaoDuong?.TanSuat,
                     x.BaoDuong?.GiaTri,
                     x.IsHoanThanh,
                     x.IsLenhHoanThanh,
                     LoaiBaoDuong = x.BaoDuong?.Name,
                     x.PhuongTien_Id,
                     x.PhuongTien?.BienSo1,
                     x.PhuongTien?.SoKM,
                     x.PhuongTien?.SoChuyenXe,
                     ChiSo = x.BaoDuong?.GiaTri,
                     ChiPhi = x?.ChiPhi > 0 ? string.Format("{0:N0}", x.ChiPhi) : "",
                     ChiPhi_TD = x?.ChiPhi_TD > 0 ? string.Format("{0:N0}", x.ChiPhi_TD) : "",
                     x.NoiDung,
                     x.KetQua,
                     lichSu = x.LichSuBaoDuong_ChiTiets.Where(a => !a.IsDeleted && a.LichSuBaoDuong_Id == x.Id
                    ).Select(x => new
                    {
                        x.Id,
                        x.LichSuBaoDuong_Id,
                        x.HangMuc?.NoiDungBaoDuong,
                        DinhMuc = string.Format("{0:N0}", x.HangMuc?.DinhMuc),
                        // x.HangMuc?.DinhMuc,
                        x.HangMuc?.LoaiBaoDuong,
                        x.HangMuc?.GhiChu,
                        x.LichSuBaoDuong?.PhuongTien?.BienSo1,
                        x.LichSuBaoDuong?.PhuongTien?.BienSo2,

                    }),
                 };
             });
            return Ok(data);
        }

        [HttpGet("BaoDuong")]
        public async Task<ActionResult> GetKH(DateTime TuNgay, DateTime DenNgay, string keyword = null)
        {
            var dataList = await _master.GetDiadiem();
            Expression<Func<LichSuBaoDuong, bool>> whereFunc = item => !item.IsDeleted
            && item.PhuongTien_Id != null
            && item.NgayDeXuatHoanThanh.Value.Date >= TuNgay && item.NgayDeXuatHoanThanh.Value.Date <= DenNgay
            && item.IsLenhHoanThanh
            && (string.IsNullOrEmpty(keyword) ||
            item.PhuongTien.BienSo1.Contains(keyword.Trim().ToLower())
            || item.PhuongTien.BienSo2.Contains(keyword.Trim().ToLower()));
            Func<IQueryable<LichSuBaoDuong>, IOrderedQueryable<LichSuBaoDuong>> orderByFunc = item => item.OrderByDescending(x => x.NgayDeXuatHoanThanh);
            string[] includes = { "BaoDuong", "PhuongTien","LichSuBaoDuong_ChiTiets.HangMuc",
            "PhuongTien.PhuTrachBoPhan","PhuongTien.DM_Loai"
            };
            var data = uow.LichSuBaoDuongs.GetAll(whereFunc, orderByFunc, includes)
             .Select(x =>
             {
                 var now = DateTime.Now;
                 var ngaySuDung = x.NgayDeXuatHoanThanh != null ? (now - x.NgayDeXuatHoanThanh)?.Days : null;
                 return new
                 {
                     x.Id,
                     x.DiaDiem_Id,
                     x.PhuongTien?.MaPhuongTien,
                     LoaiPT = x.PhuongTien?.DM_Loai?.Name,
                     Model = x.PhuongTien?.DM_Model?.Name,
                     x.PhuongTien?.DM_Model?.Option,
                     NguoiPhuTrach = x.PhuongTien?.PhuTrachBoPhan?.NhanVien,
                     SoNgaySuDung = ngaySuDung,
                     TenDiaDiem = dataList.Where(t => !x.IsDeleted && t.Id == x.DiaDiem_Id)?.FirstOrDefault()?.TenDiaDiem,
                     Ngay = string.Format("{0:dd/MM/yyyy}", x.Ngay),
                     NgayXacNhan = string.Format("{0:dd/MM/yyyy}", x.NgayXacNhan),
                     NgayDiBaoDuong = string.Format("{0:dd/MM/yyyy HH:mm}", x.NgayDiBaoDuong),
                     NgayHoanThanh = string.Format("{0:dd/MM/yyyy HH:mm} ", x.NgayDeXuatHoanThanh),
                     x.BaoDuong_Id,
                     x.BaoDuong?.TanSuat,
                     x.BaoDuong?.GiaTri,
                     x.IsHoanThanh,
                     x.IsLenhHoanThanh,
                     LoaiBaoDuong = x.BaoDuong?.Name,
                     x.PhuongTien_Id,
                     x.PhuongTien?.BienSo1,
                     x.PhuongTien?.SoKM,
                     x.PhuongTien?.SoChuyenXe,
                     x.HinhAnh,
                     x.PhuongTien?.SoKM_NgayBaoDuong,
                     ChiPhi = x?.ChiPhi > 0 ? string.Format("{0:N0}", x.ChiPhi) : "",
                     ChiPhi_TD = x?.ChiPhi_TD > 0 ? string.Format("{0:N0}", x.ChiPhi_TD) : "",
                     TongChiPhi = x.IsHoanThanh ?
                        (x.ChiPhi_TD > 0 ? string.Format("{0:N0}", x.ChiPhi_TD) : "") :
                        (x.ChiPhi > 0 ? string.Format("{0:N0}", x.ChiPhi) : ""),
                     ChiSo = x.BaoDuong?.GiaTri,
                     x.NoiDung,
                     x.KetQua,
                     x.NguoiDeXuatHoanThanh,
                     lichSu = x.LichSuBaoDuong_ChiTiets.Where(a => !a.IsDeleted && a.LichSuBaoDuong_Id == x.Id
                   ).Select(x => new
                   {
                       x.Id,
                       x.LichSuBaoDuong_Id,
                       x.HangMuc?.NoiDungBaoDuong,
                       // x.HangMuc?.DinhMuc,
                       DinhMuc = string.Format("{0:N0}", x.HangMuc?.DinhMuc),
                       x.HangMuc?.LoaiBaoDuong,
                       x.HangMuc?.GhiChu,
                       ChiPhi = string.Format("{0:N0}", x.ChiPhi),
                       // x.ChiPhi,
                       x.LichSuBaoDuong?.PhuongTien?.BienSo1,
                       x.LichSuBaoDuong?.PhuongTien?.BienSo2,
                   }),
                 };
             });
            return Ok(data);
        }

        [HttpGet("GetById")]
        public async Task<ActionResult> GetEdit(Guid? Id)
        {
            var dataList = await _master.GetDiadiem();
            Expression<Func<LichSuBaoDuong, bool>> whereFunc = item => !item.IsDeleted && item.Id == Id;
            string[] includes = { "BaoDuong", "PhuongTien" };
            var data = uow.LichSuBaoDuongs.GetAll(whereFunc, null, includes)
             .Select(x => new
             {
                 x.Id,
                 x.BaoDuong_Id,
                 TenDiaDiem = dataList.Where(t => !x.IsDeleted && t.Id == x.DiaDiem_Id)?.FirstOrDefault()?.TenDiaDiem,
                 NgayDiBaoDuong = string.Format("{0:dd/MM/yyyy}", x.NgayDiBaoDuong),
                 NgayHoanThanh = string.Format("{0:dd/MM/yyyy}", x.NgayHoanThanh),
                 x.Ngay,
                 x.NgayXacNhan,
                 x.DiaDiem_Id,
                 x.BaoDuong?.TanSuat,
                 x.BaoDuong?.GiaTri,
                 x.PhuongTien_Id,
                 x.PhuongTien?.BienSo1,
                 x.PhuongTien?.SoKM,
                 x.PhuongTien?.SoChuyenXe,
                 ChiSo = x.BaoDuong?.GiaTri,
                 ChiPhi = x?.ChiPhi > 0 ? string.Format("{0:N0}", x.ChiPhi) : "",
                 ChiPhi_TD = x?.ChiPhi_TD > 0 ? string.Format("{0:N0}", x.ChiPhi_TD) : "",
                 x.NoiDung,
                 x.KetQua,


             }).FirstOrDefault();
            return Ok(data);
        }

        [HttpPost("YeuCauBaoDuong_QuanLy")]
        public ActionResult YeuCau([FromBody] List<LichSuBaoDuong> dataList, [FromQuery] List<Guid> ids, string TenNhanVien, string MaNhanVien, Guid? User_Id, string NgayDiBaoDuong)
        {
            lock (Commons.LockObjectState)
            {
                // var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
                DateTime ngayDiBaoDuong;
                if (!DateTime.TryParseExact(NgayDiBaoDuong, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out ngayDiBaoDuong))
                {
                    return StatusCode(StatusCodes.Status409Conflict, "Ngày không đúng định dạng!");
                }
                foreach (var data in dataList)
                {
                    LichSuBaoDuong duLieu = new LichSuBaoDuong();
                    duLieu.Id = Guid.NewGuid();
                    duLieu.PhuongTien_Id = data.PhuongTien_Id;
                    duLieu.BaoDuong_Id = data.BaoDuong_Id;
                    duLieu.NguoiYeuCau_Id = User_Id;
                    duLieu.Ngay = DateTime.Now;
                    duLieu.NgayXacNhan = DateTime.Now;
                    duLieu.NgayDiBaoDuong = ngayDiBaoDuong;
                    duLieu.IsYeuCau = true;
                    duLieu.IsDuyet = true;
                    duLieu.NguoiYeuCau = TenNhanVien + "-" + MaNhanVien;
                    duLieu.NguoiXacNhan = TenNhanVien + "-" + MaNhanVien;
                    duLieu.DiaDiem_Id = data.DiaDiem_Id;
                    duLieu.CreatedDate = DateTime.Now;
                    duLieu.CreatedBy = Guid.Parse(User.Identity.Name);
                    uow.LichSuBaoDuongs.Add(duLieu);

                    var phuongtien = uow.DS_PhuongTiens.GetById(data.PhuongTien_Id);
                    if (phuongtien == null)
                    {
                        return BadRequest("Không tìm thấy phương tiện ");
                    }
                    phuongtien.LichSuBaoDuong_Id = duLieu.Id;
                    phuongtien.TinhTrang_Id = Guid.Parse("3fcfbc57-b53e-4226-8251-674c5a839b60");
                    uow.DS_PhuongTiens.Update(phuongtien);
                    var lichSuBaoDuongList = new List<LichSuBaoDuong_ChiTiet>();
                    foreach (var id in ids)
                    {
                        var chitiet = new LichSuBaoDuong_ChiTiet();
                        chitiet.Id = Guid.NewGuid();
                        chitiet.LichSuBaoDuong_Id = duLieu.Id;
                        chitiet.PhuongTien_Id = data.PhuongTien_Id;
                        chitiet.HangMuc_Id = id;
                        chitiet.CreatedDate = DateTime.Now;
                        chitiet.CreatedBy = Guid.Parse(User.Identity.Name);
                        lichSuBaoDuongList.Add(chitiet);
                    }
                    uow.LichSuBaoDuong_ChiTiets.AddRange(lichSuBaoDuongList);

                    uow.Complete();
                }
                //Ghi log truy cập
                return Ok(1);
            }
        }

        [HttpPost("YeuCauBaoDuong")]
        public ActionResult LSYeuCauThayDoiKHDiGap([FromBody] List<LichSuBaoDuong> dataList, [FromQuery] List<Guid> ids, string TenNhanVien, string MaNhanVien, Guid? User_Id, string NgayDiBaoDuong, string HinhAnh)
        {
            lock (Commons.LockObjectState)
            {
                // var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
                foreach (var data in dataList)
                {
                    DateTime? ngayDiBaoDuong = TryParseDate(NgayDiBaoDuong);
                    LichSuBaoDuong duLieu = new LichSuBaoDuong();
                    duLieu.Id = Guid.NewGuid();
                    duLieu.PhuongTien_Id = data.PhuongTien_Id;
                    duLieu.BaoDuong_Id = data.BaoDuong_Id;
                    duLieu.SoKM = data.SoKM;
                    duLieu.NguoiYeuCau_Id = User_Id;
                    duLieu.Ngay = DateTime.Now;
                    duLieu.IsYeuCau = true;
                    duLieu.HinhAnh = HinhAnh;
                    duLieu.DiaDiem_Id = data.DiaDiem_Id;
                    duLieu.NguoiYeuCau = TenNhanVien + "-" + MaNhanVien;
                    duLieu.NgayDiBaoDuong = ngayDiBaoDuong;
                    duLieu.CreatedDate = DateTime.Now;
                    duLieu.CreatedBy = Guid.Parse(User.Identity.Name);
                    uow.LichSuBaoDuongs.Add(duLieu);

                    var phuongtien = uow.DS_PhuongTiens.GetById(data.PhuongTien_Id);
                    if (phuongtien == null)
                    {
                        return BadRequest("Không tìm thấy phương tiện ");
                    }
                    phuongtien.LichSuBaoDuong_Id = duLieu.Id;
                    phuongtien.TinhTrang_Id = Guid.Parse("d85552ee-7f15-4626-870b-ea8329afb6dd");

                    uow.DS_PhuongTiens.Update(phuongtien);
                    var lichSuBaoDuongList = new List<LichSuBaoDuong_ChiTiet>();
                    foreach (var id in ids)
                    {
                        var chitiet = new LichSuBaoDuong_ChiTiet();
                        chitiet.Id = Guid.NewGuid();
                        chitiet.LichSuBaoDuong_Id = duLieu.Id;
                        chitiet.PhuongTien_Id = data.PhuongTien_Id;
                        chitiet.HangMuc_Id = id;
                        chitiet.CreatedDate = DateTime.Now;
                        chitiet.CreatedBy = Guid.Parse(User.Identity.Name);
                        lichSuBaoDuongList.Add(chitiet);
                    }
                    uow.LichSuBaoDuong_ChiTiets.AddRange(lichSuBaoDuongList);
                    uow.Complete();
                }
                //Ghi log truy cập
                return Ok(1);
            }
        }
        // [HttpPost("YeuCauBaoDuong")]
        // public ActionResult LSYeuCauThayDoiKHDiGap([FromBody] List<LichSuBaoDuong> dataList,
        //                                   [FromQuery] List<Guid> ids,
        //                                   string TenNhanVien,
        //                                   string MaNhanVien,
        //                                   Guid? User_Id)
        // {
        //     lock (Commons.LockObjectState)
        //     {
        //         try
        //         {
        //             if (dataList == null || !dataList.Any())
        //                 return BadRequest("Dữ liệu đầu vào không hợp lệ.");

        //             if (string.IsNullOrEmpty(TenNhanVien) || string.IsNullOrEmpty(MaNhanVien))
        //                 return BadRequest("Tên nhân viên và mã nhân viên không được để trống.");

        //             if (User_Id == null || User_Id == Guid.Empty)
        //                 return BadRequest("User_Id không hợp lệ.");

        //             foreach (var data in dataList)
        //             {
        //                 if (data.PhuongTien_Id == null || data.PhuongTien_Id == Guid.Empty)
        //                     return BadRequest("PhuongTien_Id không hợp lệ.");

        //                 if (data.BaoDuong_Id == null || data.BaoDuong_Id == Guid.Empty)
        //                     return BadRequest("BaoDuong_Id không hợp lệ.");

        //                 // Tạo đối tượng LichSuBaoDuong mới
        //                 var duLieu = new LichSuBaoDuong
        //                 {
        //                     Id = Guid.NewGuid(),
        //                     PhuongTien_Id = data.PhuongTien_Id,
        //                     BaoDuong_Id = data.BaoDuong_Id,
        //                     SoKM = data.SoKM,
        //                     NguoiYeuCau_Id = User_Id,
        //                     Ngay = DateTime.Now,
        //                     IsYeuCau = true,
        //                     NguoiYeuCau = $"{TenNhanVien}-{MaNhanVien}",
        //                     CreatedDate = DateTime.Now,
        //                     CreatedBy = User_Id.Value
        //                 };

        //                 // Thêm vào danh sách
        //                 uow.LichSuBaoDuongs.Add(duLieu);

        //                 // Kiểm tra phương tiện có tồn tại không
        //                 var phuongtien = uow.DS_PhuongTiens.GetById(data.PhuongTien_Id);
        //                 if (phuongtien == null)
        //                     return BadRequest($"Không tìm thấy phương tiện với ID: {data.PhuongTien_Id}");

        //                 // Cập nhật phương tiện
        //                 phuongtien.LichSuBaoDuong_Id = duLieu.Id;
        //                 phuongtien.TinhTrang_Id = Guid.Parse("d85552ee-7f15-4626-870b-ea8329afb6dd");
        //                 uow.DS_PhuongTiens.Update(phuongtien);

        //                 // Thêm danh sách chi tiết bảo dưỡng
        //                 var lichSuBaoDuongList = new List<LichSuBaoDuong_ChiTiet>();
        //                 foreach (var id in ids)
        //                 {
        //                     var validHangMucIds = uow.DM_HangMucs.GetAll(x => !x.IsDeleted).Select(x => x.Id).ToHashSet();
        //                     if (id == Guid.Empty)
        //                         return BadRequest("Một hoặc nhiều ID trong danh sách 'ids' không hợp lệ.");

        //                     var chitiet = new LichSuBaoDuong_ChiTiet
        //                     {
        //                         Id = Guid.NewGuid(),
        //                         LichSuBaoDuong_Id = duLieu.Id,
        //                         PhuongTien_Id = data.PhuongTien_Id,
        //                         HangMuc_Id = id,
        //                         CreatedDate = DateTime.Now,
        //                         CreatedBy = User_Id.Value
        //                     };
        //                     lichSuBaoDuongList.Add(chitiet);
        //                 }

        //                 uow.LichSuBaoDuong_ChiTiets.AddRange(lichSuBaoDuongList);
        //             }

        //             // Lưu dữ liệu vào DB
        //             uow.Complete();
        //             return Ok(new { message = "Lưu dữ liệu thành công." });
        //         }
        //         catch (DbUpdateException dbEx)
        //         {
        //             return StatusCode(500, new
        //             {
        //                 State = "Internal Server Error",
        //                 Msg = dbEx.Message,
        //                 InnerException = dbEx.InnerException?.Message,
        //                 StackTrace = dbEx.StackTrace
        //             });
        //         }
        //         catch (Exception ex)
        //         {
        //             return StatusCode(500, new
        //             {
        //                 State = "Internal Server Error",
        //                 Msg = ex.Message,
        //                 InnerException = ex.InnerException?.Message,
        //                 StackTrace = ex.StackTrace
        //             });
        //         }
        //     }
        // }


        [HttpPost("HuyYeuCauBaoDuong")]
        public ActionResult HuyYeuCau([FromBody] List<LichSuBaoDuong> dataList, string TenNhanVien, string MaNhanVien, Guid? User_Id, string LiDo)
        {
            lock (Commons.LockObjectState)
            {
                // var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
                foreach (var data in dataList)
                {
                    var yeuCau = uow.LichSuBaoDuongs.GetById(data.Id);
                    if (yeuCau == null)
                    {
                        return BadRequest($"Yêu cầu không tồn tại ");
                    }
                    yeuCau.LyDo = LiDo;
                    yeuCau.IsDuyet = false;
                    yeuCau.IsYeuCau = false;
                    yeuCau.TrangThai = "Đã huỷ";
                    yeuCau.IsDeleted = true;
                    yeuCau.UpdatedDate = DateTime.Now;
                    yeuCau.UpdatedBy = Guid.Parse(User.Identity.Name);
                    uow.LichSuBaoDuongs.Update(yeuCau);
                    var phuongtien = uow.DS_PhuongTiens.GetById(yeuCau.PhuongTien_Id);
                    if (phuongtien == null)
                    {
                        return BadRequest("Không tìm thấy phương tiện ");
                    }

                    phuongtien.TinhTrang_Id = Guid.Parse("90b5fae3-8fb6-43e3-8af2-6cec3627d3fc");
                    uow.DS_PhuongTiens.Update(phuongtien);
                    uow.Complete();
                }
                //Ghi log truy cập
                return Ok(1);
            }
        }

        [HttpPost("HuyYeuCauBaoDuong_Wed")]
        public async Task<ActionResult> HuyYeuCauWedAsync(Guid? id, string TenNhanVien, string MaNhanVien, Guid? User_Id, string LiDo)
        {
            // lock (Commons.LockObjectState)
            await _lock.WaitAsync(); // Đợi lấy quyền truy cập
            try
            {
                string body;
                // var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
                var yeuCau = uow.LichSuBaoDuongs.GetById(id);
                if (yeuCau == null)
                {
                    return BadRequest($"Yêu cầu không tồn tại ");
                }
                yeuCau.LyDo = LiDo;
                yeuCau.IsDuyet = false;
                yeuCau.IsYeuCau = false;
                yeuCau.TrangThai = "Đã huỷ";
                yeuCau.IsDeleted = true;
                yeuCau.UpdatedDate = DateTime.Now;
                yeuCau.UpdatedBy = Guid.Parse(User.Identity.Name);
                uow.LichSuBaoDuongs.Update(yeuCau);
                var phuongtien = uow.DS_PhuongTiens.GetById(yeuCau.PhuongTien_Id);
                if (phuongtien == null)
                {
                    return BadRequest("Không tìm thấy phương tiện ");
                }
                phuongtien.TinhTrang_Id = Guid.Parse("90b5fae3-8fb6-43e3-8af2-6cec3627d3fc");
                // phuongtien.TinhTrang_Id = Guid.Parse("d85552ee-7f15-4626-870b-ea8329afb6dd");
                uow.DS_PhuongTiens.Update(phuongtien);

                uow.Complete();
                body = "Bạn vừa bị huỷ 1 yêu cầu bảo dưỡng phương tiện";
                await thongbao.SendPushNotificationAsync(body, phuongtien?.Id);

                //Ghi log truy cập
                return Ok(1);
            }
            finally
            {
                _lock.Release(); // Giải phóng quyền truy cập
            }
        }

        [HttpPost("XacNhanYeuCauList")]
        public ActionResult XacNhanYeuCauThayDoiKHList([FromBody] List<LichSuBaoDuong> dataList, string TenNhanVien, string MaNhanVien, Guid? User_Id)
        {
            lock (Commons.LockObjectState)
            {
                // var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
                foreach (var data in dataList)
                {
                    var yeuCau = uow.LichSuBaoDuongs.GetById(data.Id);
                    if (yeuCau == null)
                    {
                        return BadRequest($"Yêu cầu không tồn tại ");
                    }

                    yeuCau.NgayXacNhan = DateTime.Now;
                    yeuCau.NguoiXacNhan_Id = User_Id;
                    yeuCau.IsDuyet = true;
                    yeuCau.NguoiXacNhan = TenNhanVien + "-" + MaNhanVien;
                    yeuCau.UpdatedDate = DateTime.Now;
                    yeuCau.UpdatedBy = Guid.Parse(User.Identity.Name);
                    uow.LichSuBaoDuongs.Update(yeuCau);
                    var phuongtien = uow.DS_PhuongTiens.GetById(yeuCau.PhuongTien_Id);
                    if (phuongtien == null)
                    {
                        return BadRequest("Không tìm thấy phương tiện ");
                    }
                    phuongtien.TinhTrang_Id = Guid.Parse("3fcfbc57-b53e-4226-8251-674c5a839b60");
                    uow.DS_PhuongTiens.Update(phuongtien);

                    uow.Complete();
                }
                //Ghi log truy cập
                return Ok(1);
            }
        }

        [HttpPost("XacNhanYeuCau")]
        public ActionResult XacNhanYeuCauThayDoiKH([FromBody] List<LichSuBaoDuong> dataList, [FromQuery] List<Guid?> ids, string NgayDiBaoDuong, string TenNhanVien, string MaNhanVien, Guid? User_Id)
        {
            lock (Commons.LockObjectState)
            {
                // var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
                foreach (var data in dataList)
                {
                    var yeuCau = uow.LichSuBaoDuongs.GetById(data.Id);
                    if (yeuCau == null)
                    {
                        return BadRequest($"Yêu cầu không tồn tại ");
                    }
                    DateTime? ngayDiBaoDuong = TryParseDate(NgayDiBaoDuong);
                    yeuCau.NgayDiBaoDuong = ngayDiBaoDuong;
                    yeuCau.NgayXacNhan = DateTime.Now;
                    yeuCau.NguoiXacNhan_Id = User_Id;
                    yeuCau.DiaDiem_Id = data.DiaDiem_Id;
                    yeuCau.IsDuyet = true;
                    yeuCau.NguoiXacNhan = TenNhanVien + "-" + MaNhanVien;
                    yeuCau.UpdatedDate = DateTime.Now;
                    yeuCau.UpdatedBy = Guid.Parse(User.Identity.Name);

                    uow.LichSuBaoDuongs.Update(yeuCau);
                    var phuongtien = uow.DS_PhuongTiens.GetById(yeuCau.PhuongTien_Id);
                    if (phuongtien == null)
                    {
                        return BadRequest("Không tìm thấy phương tiện ");
                    }

                    phuongtien.TinhTrang_Id = Guid.Parse("3fcfbc57-b53e-4226-8251-674c5a839b60");
                    uow.DS_PhuongTiens.Update(phuongtien);
                    var existingItems = uow.LichSuBaoDuong_ChiTiets.GetAll(x => !x.IsDeleted && x.LichSuBaoDuong_Id == yeuCau.Id).Select(x => x.HangMuc_Id).ToList();

                    // Lọc ra các hạng mục mới cần thêm
                    var newItems = ids.Except(existingItems).ToList();
                    var removedItems = existingItems.Except(ids).ToList();

                    // Thêm hạng mục mới vào bảng LichSuBaoDuongChiTiet
                    foreach (var hangMucId in newItems)
                    {
                        uow.LichSuBaoDuong_ChiTiets.Add(new LichSuBaoDuong_ChiTiet
                        {
                            Id = Guid.NewGuid(),
                            LichSuBaoDuong_Id = yeuCau.Id,
                            PhuongTien_Id = yeuCau.PhuongTien_Id,
                            HangMuc_Id = hangMucId,
                            CreatedDate = DateTime.Now,
                            CreatedBy = Guid.Parse(User.Identity.Name)
                        });
                    }
                    if (removedItems.Any()) // Kiểm tra xem có mục nào cần xóa không
                    {
                        var itemsToUpdate = uow.LichSuBaoDuong_ChiTiets
                            .GetAll(x => x.LichSuBaoDuong_Id == yeuCau.Id && removedItems.Contains(x.HangMuc_Id))
                            .ToList();
                        foreach (var item in itemsToUpdate)
                        {
                            item.IsDeleted = true; // Đánh dấu là đã xóa thay vì xóa khỏi DB
                            item.UpdatedDate = DateTime.Now; // Cập nhật thời gian chỉnh sửa
                            item.UpdatedBy = Guid.Parse(User.Identity.Name);
                        }
                        uow.LichSuBaoDuong_ChiTiets.UpdateRange(itemsToUpdate); // Xóa danh sách cùng lúc
                    }

                    uow.Complete();
                }
                //Ghi log truy cập
                return Ok(1);
            }
        }
        private DateTime? TryParseDate(string dateStr)
        {
            string[] formats = { "dd/MM/yyyy", "dd/MM/yyyy HH:mm" };
            return DateTime.TryParseExact(dateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)
                ? date
                : null;
        }
        [HttpPost("HuyXacNhanYeuCau")]
        public ActionResult XacNhanYeuCauWed(Guid? id, string TenNhanVien, string MaNhanVien, Guid? User_Id)
        {
            lock (Commons.LockObjectState)
            {
                // var yeuCau = uow.LichSuThayDoiKeHoachs.GetAll(x => !x.IsDeleted && x.KeHoachGiaoXe_Id == data.KeHoachGiaoXe_Id && x.TrangThai == null).OrderByDescending(x => x.ThoiGianYC).FirstOrDefault();
                var yeuCau = uow.LichSuBaoDuongs.GetById(id);
                if (yeuCau == null)
                {
                    return BadRequest($"Yêu cầu không tồn tại ");
                }
                yeuCau.IsDuyet = false;
                yeuCau.NguoiHuyDuyet = TenNhanVien + "-" + MaNhanVien;
                yeuCau.NguoiHuyDuyet_Id = Guid.Parse(User.Identity.Name);
                yeuCau.UpdatedDate = DateTime.Now;
                yeuCau.UpdatedBy = Guid.Parse(User.Identity.Name);
                uow.LichSuBaoDuongs.Update(yeuCau);
                var phuongtien = uow.DS_PhuongTiens.GetById(yeuCau.PhuongTien_Id);
                if (phuongtien == null)
                {
                    return BadRequest("Không tìm thấy phương tiện ");
                }
                phuongtien.TinhTrang_Id = Guid.Parse("d85552ee-7f15-4626-870b-ea8329afb6dd");
                // phuongtien.TinhTrang_Id = Guid.Parse("d85552ee-7f15-4626-870b-ea8329afb6dd");
                uow.DS_PhuongTiens.Update(phuongtien);

                uow.Complete();

                //Ghi log truy cập
                return Ok(1);
            }
        }
        [HttpPost("XacNhanYeuCau_Wed")]
        public async Task<ActionResult> XacNhanYeuCauWedAsync(List<Guid?> ids, Guid? id, Guid? DiaDiem_Id, string NgayDiBaoDuong, string TenNhanVien, string MaNhanVien, Guid? User_Id)
        {
            // lock (Commons.LockObjectState)
            await _lock.WaitAsync(); // Đợi lấy quyền truy cập
            try
            {
                string body;
                // var yeuCau = uow.LichSuThayDoiKeHoachs.GetAll(x => !x.IsDeleted && x.KeHoachGiaoXe_Id == data.KeHoachGiaoXe_Id && x.TrangThai == null).OrderByDescending(x => x.ThoiGianYC).FirstOrDefault();
                var yeuCau = uow.LichSuBaoDuongs.GetById(id);
                if (yeuCau == null)
                {
                    return BadRequest($"Yêu cầu không tồn tại ");
                }
                var phuongtien = uow.DS_PhuongTiens.GetById(yeuCau.PhuongTien_Id);
                if (phuongtien == null)
                {
                    return BadRequest("Không tìm thấy phương tiện ");
                }
                DateTime? ngayDiBaoDuong = TryParseDate(NgayDiBaoDuong);

                yeuCau.NgayXacNhan = DateTime.Now;
                yeuCau.NguoiXacNhan_Id = Guid.Parse(User.Identity.Name);
                yeuCau.IsDuyet = !yeuCau.IsDuyet;
                if (yeuCau.IsDuyet)
                {
                    yeuCau.NguoiXacNhan = TenNhanVien + "-" + MaNhanVien;
                    yeuCau.NguoiXacNhan_Id = Guid.Parse(User.Identity.Name);
                    yeuCau.NgayDiBaoDuong = ngayDiBaoDuong;
                    yeuCau.DiaDiem_Id = DiaDiem_Id;
                    phuongtien.TinhTrang_Id = Guid.Parse("3fcfbc57-b53e-4226-8251-674c5a839b60");
                    var existingItems = uow.LichSuBaoDuong_ChiTiets.GetAll(x => !x.IsDeleted && x.LichSuBaoDuong_Id == yeuCau.Id).Select(x => x.HangMuc_Id).ToList();

                    // Lọc ra các hạng mục mới cần thêm
                    var newItems = ids.Except(existingItems).ToList();
                    var removedItems = existingItems.Except(ids).ToList();

                    // Thêm hạng mục mới vào bảng LichSuBaoDuongChiTiet
                    foreach (var hangMucId in newItems)
                    {
                        uow.LichSuBaoDuong_ChiTiets.Add(new LichSuBaoDuong_ChiTiet
                        {
                            Id = Guid.NewGuid(),
                            PhuongTien_Id = yeuCau.PhuongTien_Id,
                            LichSuBaoDuong_Id = yeuCau.Id,
                            HangMuc_Id = hangMucId,
                            CreatedDate = DateTime.Now,
                            CreatedBy = Guid.Parse(User.Identity.Name)
                        });
                    }
                    if (removedItems.Any()) // Kiểm tra xem có mục nào cần xóa không
                    {
                        var itemsToUpdate = uow.LichSuBaoDuong_ChiTiets
                            .GetAll(x => x.LichSuBaoDuong_Id == yeuCau.Id && removedItems.Contains(x.HangMuc_Id))
                            .ToList();

                        foreach (var item in itemsToUpdate)
                        {
                            item.IsDeleted = true; // Đánh dấu là đã xóa thay vì xóa khỏi DB
                            item.UpdatedDate = DateTime.Now; // Cập nhật thời gian chỉnh sửa
                            item.UpdatedBy = Guid.Parse(User.Identity.Name);
                        }
                        uow.LichSuBaoDuong_ChiTiets.UpdateRange(itemsToUpdate); // Xóa danh sách cùng lúc
                    }
                    body = "Bạn vừa được xác nhận 1 yêu cầu bảo dưỡng phương tiện";

                }
                else
                {
                    yeuCau.NguoiHuyDuyet = TenNhanVien + "-" + MaNhanVien;
                    yeuCau.NguoiHuyDuyet_Id = Guid.Parse(User.Identity.Name);
                    phuongtien.TinhTrang_Id = Guid.Parse("d85552ee-7f15-4626-870b-ea8329afb6dd");
                    body = "Bạn vừa bị huỷ xác nhận 1 yêu cầu bảo dưỡng phương tiện";
                }

                yeuCau.UpdatedDate = DateTime.Now;
                yeuCau.UpdatedBy = Guid.Parse(User.Identity.Name);
                uow.LichSuBaoDuongs.Update(yeuCau);

                // phuongtien.TinhTrang_Id = Guid.Parse("d85552ee-7f15-4626-870b-ea8329afb6dd");
                uow.DS_PhuongTiens.Update(phuongtien);

                uow.Complete();
                await thongbao.SendPushNotificationAsync(body, phuongtien?.Id);

                //Ghi log truy cập
                return Ok(1);
            }
            finally
            {
                _lock.Release(); // Giải phóng quyền truy cập
            }
        }

        [HttpPost("DiBaoDuong")]
        public ActionResult XacNhanDiBaoDuong(LichSuBaoDuong data, string TenNhanVien, string MaNhanVien, Guid? User_Id)
        {
            lock (Commons.LockObjectState)
            {
                // var exit = userManager.FindByIdAsync(User.Identity.Name).Result;

                var yeuCau = uow.LichSuBaoDuongs.GetById(data.Id);
                if (yeuCau == null)
                {
                    return BadRequest($"Yêu cầu không tồn tại ");
                }

                yeuCau.NgayDiBaoDuong = DateTime.Now;
                yeuCau.NguoiXacNhan_Id = User_Id;
                yeuCau.IsBaoDuong = true;
                // yeuCau.DiaDiem_Id = data.DiaDiem_Id;
                yeuCau.NguoiDiBaoDuong = TenNhanVien + "-" + MaNhanVien;
                yeuCau.UpdatedDate = DateTime.Now;
                yeuCau.UpdatedBy = Guid.Parse(User.Identity.Name);
                uow.LichSuBaoDuongs.Update(yeuCau);
                var phuongtien = uow.DS_PhuongTiens.GetById(yeuCau.PhuongTien_Id);
                if (phuongtien == null)
                {
                    return BadRequest("Không tìm thấy phương tiện ");
                }

                phuongtien.TinhTrang_Id = Guid.Parse("d7ed914c-425a-4909-59c3-08dd29516aa7");
                // phuongtien.TinhTrang_Id = Guid.Parse("d85552ee-7f15-4626-870b-ea8329afb6dd");
                uow.DS_PhuongTiens.Update(phuongtien);



                uow.Complete();

                //Ghi log truy cập
                return Ok(1);
            }
        }
        public class ChiPhiModel
        {
            public Guid? HangMuc_Id { get; set; }
            public int GiaTri { get; set; }  // Giá trị chi phí bảo dưỡng
        }
        public class XacNhanHoanThanhBaoDuongDTO
        {
            public LichSuBaoDuong Data { get; set; }
            public List<ChiPhiModel> ChiPhis { get; set; }

        }

        [HttpPost("LenhHoanThanhBaoDuong")]
        public ActionResult XacNhanHoanThanhBaoDuong([FromBody] XacNhanHoanThanhBaoDuongDTO request, string TenNhanVien, string MaNhanVien, Guid? User_Id, int SoKM, string File)
        {
            lock (Commons.LockObjectState)
            {
                // var exit = userManager.FindByIdAsync(User.Identity.Name).Result;

                var yeuCau = uow.LichSuBaoDuongs.GetById(request.Data.Id);
                if (yeuCau == null)
                {
                    return BadRequest($"Yêu cầu không tồn tại ");
                }

                yeuCau.NgayDeXuatHoanThanh = DateTime.Now;
                yeuCau.NguoiDeXuatHoanThanh_Id = User_Id;
                yeuCau.IsLenhHoanThanh = true;
                yeuCau.GhiChu = request.Data.KetQua;
                yeuCau.NguoiDeXuatHoanThanh = TenNhanVien + "-" + MaNhanVien;
                yeuCau.HinhAnh = File;
                yeuCau.SoKM = SoKM;
                yeuCau.ChiPhi = request.ChiPhis.Sum(chiphi => chiphi.GiaTri);
                yeuCau.UpdatedDate = DateTime.Now;
                yeuCau.UpdatedBy = Guid.Parse(User.Identity.Name);
                uow.LichSuBaoDuongs.Update(yeuCau);

                var lichsu_chitiet = uow.LichSuBaoDuong_ChiTiets.GetAll(x => !x.IsDeleted && x.LichSuBaoDuong_Id == yeuCau.Id)
                 .Select(x => x.HangMuc_Id)
                 .ToList();
                var lichsu = uow.ThongTinTheoHangMucs.GetAll(x => !x.IsDeleted && x.PhuongTien_Id == yeuCau.PhuongTien_Id && lichsu_chitiet.Contains(x.HangMuc_Id));
                if (lichsu.Any())
                {
                    foreach (var item in lichsu)
                    {
                        int tongChiPhiHienTai = item?.TongChiPhi_TD ?? 0;
                        // Tính tổng chi phí mới cho từng hạng mục
                        int tongChiPhiMoi = tongChiPhiHienTai + request.ChiPhis
                            .Where(chiphi => chiphi.HangMuc_Id == item.HangMuc_Id) // Lọc theo hạng mục
                            .Sum(chiphi => chiphi.GiaTri);
                        item.GiaTriBaoDuong = SoKM; // Cập nhật giá trị mới
                        item.TongChiPhi = tongChiPhiMoi;
                        item.UpdatedDate = DateTime.Now;
                        item.UpdatedBy = Guid.Parse(User.Identity.Name);
                        // uow.ThongTinTheoHangMucs.Update(item);
                    }
                    uow.ThongTinTheoHangMucs.UpdateRange(lichsu);
                }


                var lichsu_chitiet2 = uow.LichSuBaoDuong_ChiTiets
           .GetAll(x => !x.IsDeleted && x.LichSuBaoDuong_Id == yeuCau.Id)
           .ToList();

                // Cập nhật chi phí từng hạng mục
                foreach (var chiphi in request.ChiPhis)
                {
                    var hangMuc = lichsu_chitiet2.FirstOrDefault(x => x.HangMuc_Id == chiphi.HangMuc_Id);
                    if (hangMuc != null)
                    {
                        hangMuc.ChiPhi = chiphi.GiaTri;
                        hangMuc.UpdatedDate = DateTime.Now;
                        hangMuc.UpdatedBy = Guid.Parse(User.Identity.Name);
                    }
                }
                uow.LichSuBaoDuong_ChiTiets.UpdateRange(lichsu_chitiet2);

                var phuongtien = uow.DS_PhuongTiens.GetById(yeuCau.PhuongTien_Id);
                if (phuongtien == null)
                {
                    return BadRequest("Không tìm thấy phương tiện ");
                }
                else
                {
                    phuongtien.UpdatedBy = Guid.Parse(User.Identity.Name);
                    phuongtien.UpdatedDate = DateTime.Now;
                    phuongtien.SoKM_NgayBaoDuong = SoKM;
                    phuongtien.TinhTrang_Id = Guid.Parse("dba88246-a596-423d-acc3-4a62e2286ab1");
                    uow.DS_PhuongTiens.Update(phuongtien);
                }

                uow.Complete();


                //Ghi log truy cập
                return Ok(1);
            }
        }

        [HttpPost("HoanThanhBaoDuong")]
        public ActionResult HoanThanhBaoDuong([FromBody] XacNhanHoanThanhBaoDuongDTO request, string TenNhanVien, string MaNhanVien, Guid? User_Id)
        {
            lock (Commons.LockObjectState)
            {
                // var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
                var yeuCau = uow.LichSuBaoDuongs.GetById(request.Data.Id);
                if (yeuCau == null)
                {
                    return BadRequest($"Yêu cầu không tồn tại ");
                }
                yeuCau.NgayHoanThanh = DateTime.Now;
                yeuCau.NguoiXacNhanHoanThanh_Id = User_Id;
                yeuCau.IsHoanThanh = true;
                yeuCau.NguoiXacNhanHoanThanh = TenNhanVien + "-" + MaNhanVien;
                yeuCau.ChiPhi_TD = request.ChiPhis.Sum(chiphi => chiphi.GiaTri);
                yeuCau.UpdatedDate = DateTime.Now;
                yeuCau.UpdatedBy = Guid.Parse(User.Identity.Name);
                uow.LichSuBaoDuongs.Update(yeuCau);

                var lichsu_chitiet = uow.LichSuBaoDuong_ChiTiets.GetAll(x => !x.IsDeleted && x.LichSuBaoDuong_Id == yeuCau.Id)
               .Select(x => x.HangMuc_Id)
               .ToList();
                var lichsu = uow.ThongTinTheoHangMucs.GetAll(x => !x.IsDeleted && x.PhuongTien_Id == yeuCau.PhuongTien_Id && lichsu_chitiet.Contains(x.HangMuc_Id)
                );
                if (lichsu.Any())
                {
                    foreach (var item in lichsu)
                    {
                        int tongChiPhiHienTai = item?.TongChiPhi_TD ?? 0;
                        // Tính tổng chi phí mới cho từng hạng mục
                        int tongChiPhiMoi = tongChiPhiHienTai + request.ChiPhis
                            .Where(chiphi => chiphi.HangMuc_Id == item.HangMuc_Id) // Lọc theo hạng mục
                            .Sum(chiphi => chiphi.GiaTri);
                        item.TongChiPhi_TD = tongChiPhiMoi;
                        item.UpdatedDate = DateTime.Now;
                        item.UpdatedBy = Guid.Parse(User.Identity.Name);
                        // uow.ThongTinTheoHangMucs.Update(item);
                    }
                    uow.ThongTinTheoHangMucs.UpdateRange(lichsu);
                }


                var lichsu_chitiet2 = uow.LichSuBaoDuong_ChiTiets
           .GetAll(x => !x.IsDeleted && x.LichSuBaoDuong_Id == yeuCau.Id)
           .ToList();
                foreach (var chiphi in request.ChiPhis)
                {
                    var hangMuc = lichsu_chitiet2.FirstOrDefault(x => x.HangMuc_Id == chiphi.HangMuc_Id);
                    if (hangMuc != null)
                    {
                        hangMuc.ChiPhi = chiphi.GiaTri;
                        hangMuc.UpdatedDate = DateTime.Now;
                        hangMuc.UpdatedBy = Guid.Parse(User.Identity.Name);
                    }
                }
                uow.LichSuBaoDuong_ChiTiets.UpdateRange(lichsu_chitiet2);

                var phuongtien = uow.DS_PhuongTiens.GetById(yeuCau.PhuongTien_Id);
                if (phuongtien == null)
                {
                    return BadRequest("Không tìm thấy phương tiện ");
                }
                else
                {
                    phuongtien.UpdatedBy = Guid.Parse(User.Identity.Name);
                    phuongtien.UpdatedDate = DateTime.Now;
                    phuongtien.TinhTrang_Id = Guid.Parse("65f0a161-31bd-4ae6-b487-e3c95fea3dc4");
                    uow.DS_PhuongTiens.Update(phuongtien);
                }
                uow.Complete();

                //Ghi log truy cập
                return Ok(1);
            }
        }

        [HttpPost("HoanThanhBaoDuong_Wed")]
        public async Task<ActionResult> HoanThanhBaoDuong_WedAsync([FromBody] XacNhanHoanThanhBaoDuongDTO request, string TenNhanVien, string MaNhanVien, Guid? User_Id)
        {

            // lock (Commons.LockObjectState)
            await _lock.WaitAsync(); // Đợi lấy quyền truy cập
            try
            {
                string body;
                // var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
                var yeuCau = uow.LichSuBaoDuongs.GetById(request.Data.Id);
                if (yeuCau == null)
                {
                    return BadRequest($"Yêu cầu không tồn tại ");
                }
                yeuCau.NgayHoanThanh = DateTime.Now;
                yeuCau.NguoiXacNhanHoanThanh_Id = User_Id;
                yeuCau.IsHoanThanh = true;
                yeuCau.NguoiXacNhanHoanThanh = TenNhanVien + "-" + MaNhanVien;
                yeuCau.ChiPhi_TD = request.ChiPhis.Sum(chiphi => chiphi.GiaTri);
                yeuCau.UpdatedDate = DateTime.Now;
                yeuCau.UpdatedBy = Guid.Parse(User.Identity.Name);
                uow.LichSuBaoDuongs.Update(yeuCau);

                var lichsu_chitiet = uow.LichSuBaoDuong_ChiTiets.GetAll(x => !x.IsDeleted && x.LichSuBaoDuong_Id == yeuCau.Id)
               .Select(x => x.HangMuc_Id)
               .ToList();
                var lichsu = uow.ThongTinTheoHangMucs.GetAll(x => !x.IsDeleted && x.PhuongTien_Id == yeuCau.PhuongTien_Id && lichsu_chitiet.Contains(x.HangMuc_Id)
                );
                if (lichsu.Any())
                {
                    foreach (var item in lichsu)
                    {
                        int tongChiPhiHienTai = item?.TongChiPhi_TD ?? 0;
                        // Tính tổng chi phí mới cho từng hạng mục
                        int tongChiPhiMoi = tongChiPhiHienTai + request.ChiPhis
                            .Where(chiphi => chiphi.HangMuc_Id == item.HangMuc_Id) // Lọc theo hạng mục
                            .Sum(chiphi => chiphi.GiaTri);
                        item.TongChiPhi_TD = tongChiPhiMoi;
                        item.UpdatedDate = DateTime.Now;
                        item.UpdatedBy = Guid.Parse(User.Identity.Name);
                        // uow.ThongTinTheoHangMucs.Update(item);
                    }
                    uow.ThongTinTheoHangMucs.UpdateRange(lichsu);
                }


                var lichsu_chitiet2 = uow.LichSuBaoDuong_ChiTiets
           .GetAll(x => !x.IsDeleted && x.LichSuBaoDuong_Id == yeuCau.Id)
           .ToList();
                foreach (var chiphi in request.ChiPhis)
                {
                    var hangMuc = lichsu_chitiet2.FirstOrDefault(x => x.HangMuc_Id == chiphi.HangMuc_Id);
                    if (hangMuc != null)
                    {
                        hangMuc.ChiPhi = chiphi.GiaTri;
                        hangMuc.UpdatedDate = DateTime.Now;
                        hangMuc.UpdatedBy = Guid.Parse(User.Identity.Name);
                    }
                }
                uow.LichSuBaoDuong_ChiTiets.UpdateRange(lichsu_chitiet2);

                var phuongtien = uow.DS_PhuongTiens.GetById(yeuCau.PhuongTien_Id);
                if (phuongtien == null)
                {
                    return BadRequest("Không tìm thấy phương tiện ");
                }
                else
                {
                    phuongtien.UpdatedBy = Guid.Parse(User.Identity.Name);
                    phuongtien.UpdatedDate = DateTime.Now;
                    phuongtien.TinhTrang_Id = Guid.Parse("65f0a161-31bd-4ae6-b487-e3c95fea3dc4");
                    uow.DS_PhuongTiens.Update(phuongtien);
                }
                uow.Complete();
                body = "Bạn vừa được xác nhận 1 đề xuất hoàn thành bảo dưỡng phương tiện";
                await thongbao.SendPushNotificationAsync(body, phuongtien?.Id);
                //Ghi log truy cập
                return Ok(1);
            }
            finally
            {
                _lock.Release(); // Giải phóng quyền truy cập
            }
        }


        [HttpPost("HuyHoanThanhBaoDuong")]
        public async Task<ActionResult> HuyHoanThanhBaoDuongAsync(Guid? Id, string TenNhanVien, string MaNhanVien, Guid? User_Id)
        {
            // lock (Commons.LockObjectState)
            await _lock.WaitAsync(); // Đợi lấy quyền truy cập
            try
            {
                string body;
                // var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
                var yeuCau = uow.LichSuBaoDuongs.GetById(Id);
                if (yeuCau == null)
                {
                    return BadRequest($"Yêu cầu không tồn tại ");
                }
                yeuCau.NgayHoanThanh = DateTime.Now;
                yeuCau.NguoiXacNhanHoanThanh_Id = User_Id;
                yeuCau.IsHoanThanh = false;
                yeuCau.NguoiXacNhanHoanThanh = TenNhanVien + "-" + MaNhanVien;
                yeuCau.UpdatedDate = DateTime.Now;
                yeuCau.UpdatedBy = Guid.Parse(User.Identity.Name);
                uow.LichSuBaoDuongs.Update(yeuCau);
                var phuongtien = uow.DS_PhuongTiens.GetById(yeuCau.PhuongTien_Id);
                if (phuongtien == null)
                {
                    return BadRequest("Không tìm thấy phương tiện ");
                }
                else
                {
                    phuongtien.UpdatedBy = Guid.Parse(User.Identity.Name);
                    phuongtien.UpdatedDate = DateTime.Now;
                    phuongtien.TinhTrang_Id = Guid.Parse("dba88246-a596-423d-acc3-4a62e2286ab1");
                    uow.DS_PhuongTiens.Update(phuongtien);
                }

                uow.Complete();
                body = "Bạn vừa bị huỷ xác nhận 1 hoàn thành bảo dưỡng phương tiện";
                await thongbao.SendPushNotificationAsync(body, phuongtien?.Id);

                //Ghi log truy cập
                return Ok(1);
            }
            finally
            {
                _lock.Release(); // Giải phóng quyền truy cập
            }
        }

        [HttpPost("Read_Excel")]
        public async Task<ActionResult> Read_Excel(IFormFile file)
        {

            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            DateTime dt = DateTime.Now;
            // Rename file
            string fileName = (long)timeSpan.TotalSeconds + "_" + Commons.TiengVietKhongDau(file.FileName);
            string fileExt = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
            string[] supportedTypes = new[] { "xls", "xlsx" };
            if (supportedTypes.Contains(fileExt))
            {
                var dataList = await _master.GetDiadiem();
                var phuongtien = uow.DS_PhuongTiens.GetAll(x => !x.IsDeleted);
                var baoduong = uow.DM_Models.GetAll(x => !x.IsDeleted);

                string webRootPath = environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                string fullPath = Path.Combine(webRootPath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                byte[] file_byte = System.IO.File.ReadAllBytes(fullPath);
                //Kiểm tra tồn tại file và xóa
                System.IO.File.Delete(fullPath);
                using (MemoryStream ms = new MemoryStream(file_byte))
                using (ExcelPackage package = new ExcelPackage(ms))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    int rowCount = worksheet.Dimension.Rows;
                    var list_datas = new List<ImportMMS_LichSu>();
                    for (int i = 2; i <= rowCount; i++)
                    {
                        if (worksheet.Cells[i, 3].Value == null)
                        {
                            // Nếu không có dữ liệu, dừng vòng lặp
                            break;
                        }
                        object NgayBatDau = worksheet.Cells[i, 1].Value;
                        object BienSo1 = worksheet.Cells[i, 2].Value;
                        object BienSo2 = worksheet.Cells[i, 3].Value;
                        object LoaiBaoDuong = worksheet.Cells[i, 3].Value;
                        object TanSuat = worksheet.Cells[i, 5].Value;
                        object GiaTri = worksheet.Cells[i, 6].Value;
                        object NoiDung = worksheet.Cells[i, 7].Value;
                        object KetQua = worksheet.Cells[i, 8].Value;
                        object DiaDiem = worksheet.Cells[i, 9].Value;
                        object ChiPhi = worksheet.Cells[i, 10].Value;

                        DateTime baseDate = new DateTime(1900, 1, 1);
                        int excelDate = 45329;
                        var lst_Lois = new List<string>();
                        var info = new ImportMMS_LichSu();
                        info.Id = Guid.NewGuid();
                        info.BienSo1 = BienSo1?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.BienSo2 = BienSo2?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";

                        info.LoaiBaoDuong = LoaiBaoDuong?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.TanSuat = TanSuat?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.GiaTri = GiaTri?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.NoiDung = NoiDung?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.KetQua = GiaTri?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.MaDiaDiem = DiaDiem?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.ChiPhi = ChiPhi?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";

                        DateTime ConvertFromExcelDate(int excelDate, DateTime baseDate)
                        {
                            return baseDate.AddDays(excelDate - 2);
                        }
                        // Hàm xử lý chuỗi ngày
                        string ProcessDateString(string dateString, DateTime baseDate)
                        {
                            if (dateString == "#N/A" || string.IsNullOrWhiteSpace(dateString))
                            {
                                return "";
                            }
                            if (int.TryParse(dateString, out int excelDate))
                            {
                                DateTime date = ConvertFromExcelDate(excelDate, baseDate);
                                return date.ToString("dd/MM/yyyy");
                            }
                            if (DateTime.TryParse(dateString, out DateTime parsedDate))
                            {
                                return parsedDate.ToString("dd/MM/yyyy");
                            }

                            // Xử lý khi ngày không hợp lệ
                            info.IsLoi = true;
                            lst_Lois.Add($"Lỗi định dạng ngày không hợp lệ {dateString}");
                            return "";
                        }

                        var ngayBatDau = NgayBatDau?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.NgayBaoDuong = ProcessDateString(ngayBatDau, baseDate);

                        if (string.IsNullOrEmpty(info.BienSo1))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Biển số không được để trống");
                        }
                        else
                        {
                            var info_phuongtien = phuongtien.Where(x => x.BienSo1 == info.BienSo1.ToUpper() || x.BienSo2 == info.BienSo2.ToUpper()).FirstOrDefault();
                            if (info_phuongtien == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo biển số trong danh sách Phương Tiện");
                            }
                            else
                            {
                                info.PhuongTien_Id = info_phuongtien.Id;

                            }

                        }
                        if (string.IsNullOrEmpty(info.LoaiBaoDuong))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Loại bảo dưỡng không được để trống");
                        }
                        else
                        {
                            var info_baoduong = baoduong.Where(x => x.Name.ToLower() == info.LoaiBaoDuong.ToLower()).FirstOrDefault();
                            if (info_baoduong == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo loại trong danh mục Bảo Dưỡng");
                            }
                            else
                            {
                                info.BaoDuong_Id = info_baoduong.Id;

                            }

                        }
                        // if (string.IsNullOrEmpty(info.MaDiaDiem))
                        // {
                        //     info.IsLoi = true;
                        //     lst_Lois.Add("Dia diem không được để trống");
                        // }
                        // else
                        // {
                        //     var info_diadiem = dataList.Where(x => x.MaDiaDiem.ToLower() == info.MaDiaDiem.ToLower()).FirstOrDefault();
                        //     if (info_diadiem == null)
                        //     {
                        //         info.IsLoi = true;
                        //         lst_Lois.Add("Chưa tạo dia diem trong danh mục Bảo Dưỡng");
                        //     }
                        //     else
                        //     {
                        //         info.DiaDiem_Id = info_diadiem.Id;

                        //     }

                        // }




                        info.lst_Lois = lst_Lois;
                        list_datas.Add(info);
                    }

                    list_datas = list_datas.OrderBy(x => x.IsLoi).ToList();
                    //Kiểm tra trùng dữ liệu khi không có lỗi đầu vào
                    if (list_datas.Where(x => x.IsLoi).Count() == 0)
                    {

                        var processedBienSos = new HashSet<string>();
                        foreach (var item in list_datas)
                        {
                            if (!string.IsNullOrEmpty(item.LoaiBaoDuong))
                            {
                                if (DuplicateSoKhungs(list_datas.Where(x => x.LoaiBaoDuong == item.LoaiBaoDuong).ToList()) > 0)
                                {
                                    item.IsLoi = true;
                                    item.lst_Lois.Add($"Số Khung {item.LoaiBaoDuong} bị trùng lặp");
                                }
                            }
                        }
                        list_datas = list_datas.OrderBy(x => x.IsLoi).ToList();
                        return Ok(new { errorCount = list_datas.Where(x => x.IsLoi).Count(), rowCount = list_datas.Count(), list_datas });
                    }
                    else
                    {
                        return Ok(new { errorCount = list_datas.Where(x => x.IsLoi).Count(), rowCount = list_datas.Count(), list_datas });
                    }
                }
            }
            else
                return BadRequest("Định dạng tệp tin không cho phép");

        }
        private int DuplicateSoKhungs(List<ImportMMS_LichSu> lst)
        {
            return lst.GroupBy(p => new { p.LoaiBaoDuong }).Where(p => p.Count() > 1).Count();
        }

        [HttpPost("Save_Import")]
        public ActionResult Post_SaveImport(List<ImportMMS_LichSu> DH)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                IFormatProvider fm = new CultureInfo("en-US", true);

                foreach (var item in DH)
                {


                    DateTime? ngayBatDau = null;
                    if (DateTime.TryParseExact(item.NgayBaoDuong, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime tempNgayDenKV))
                    {
                        ngayBatDau = tempNgayDenKV;
                    }

                    var exit = uow.LichSuBaoDuongs.GetSingle(x => !x.IsDeleted && x.PhuongTien_Id != null && x.PhuongTien.BienSo1 == item.BienSo1.ToUpper());
                    if (exit == null)
                    {
                        uow.LichSuBaoDuongs.Add(new LichSuBaoDuong
                        {
                            PhuongTien_Id = item.PhuongTien_Id,
                            BaoDuong_Id = item.BaoDuong_Id,
                            DiaDiem_Id = item.DiaDiem_Id,
                            NoiDung = item.NoiDung,
                            KetQua = item.KetQua,
                            ChiPhi = int.Parse(item.ChiPhi),
                            NgayHoanThanh = ngayBatDau,
                            IsYeuCau = true,
                            IsDuyet = true,
                            IsBaoDuong = true,
                            IsHoanThanh = true,
                            NguoiXacNhanHoanThanh = item.NguoiXacNhanHoanThanh,
                            NguoiXacNhanHoanThanh_Id = Guid.Parse(User.Identity.Name),
                            CreatedDate = DateTime.Now,
                            CreatedBy = Guid.Parse(User.Identity.Name),
                        });
                    }
                    else
                    {
                        // var exit = uow.KeHoachGiaoXes.GetSingle(x => x.SoKhung.ToLower() == item.SoKhung.ToLower());
                        exit.PhuongTien_Id = item.PhuongTien_Id;
                        exit.BaoDuong_Id = item.BaoDuong_Id;
                        exit.DiaDiem_Id = item.DiaDiem_Id;
                        exit.NoiDung = item.NoiDung;
                        exit.KetQua = item.KetQua;
                        exit.IsYeuCau = true;
                        exit.IsDuyet = true;
                        exit.IsBaoDuong = true;
                        exit.IsHoanThanh = true;
                        exit.NguoiXacNhanHoanThanh = item.NguoiXacNhanHoanThanh;
                        exit.ChiPhi = int.Parse(item.ChiPhi);
                        exit.NgayHoanThanh = ngayBatDau;
                        exit.NguoiXacNhanHoanThanh_Id = Guid.Parse(User.Identity.Name);
                        exit.UpdatedDate = DateTime.Now;
                        exit.UpdatedBy = Guid.Parse(User.Identity.Name);
                        uow.LichSuBaoDuongs.Update(exit);
                    }
                }
                uow.Complete();
                return StatusCode(StatusCodes.Status200OK);
            }
        }
        [HttpGet("FileMau")]
        public ActionResult FileMauTM_DB()
        {
            string fullFilePath = Path.Combine(Directory.GetParent(environment.ContentRootPath).FullName, "Uploads/Templates/FileMau_LichSuBaoDuong.xlsx");
            string fileName = "FileMau_LichSuBaoDuong_" + DateTime.Now.ToString("HHmmss") + ".xlsx";
            using (ExcelPackage package = new ExcelPackage(new FileInfo(fullFilePath)))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    package.Workbook.Worksheets.Add("Sheet1");
                }
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];


                return Ok(new { data = package.GetAsByteArray(), fileName });
            }
        }


        [HttpGet("Adsun")]
        public ActionResult Adsun(string keyword)
        {
            var claims = User.Claims;
            var maNhanVien = claims.FirstOrDefault(c => c.Type == "MaNhanVien")?.Value;
            var data = uow.Adsuns.GetAll(t => !t.IsDeleted).Select(x => new
            {
                x.Id,
                x.Plate,
                x.GroupName
            });
            return Ok(data);
        }

        [HttpPost]
        public ActionResult Post(LichSuBaoDuong data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // if (uow.LichSuBaoDuongs.Exists(x => !x.IsDeleted && x.PhuongTien.BienSo1 == data.PhuongTien.BienSo1 ))
                //     return StatusCode(StatusCodes.Status409Conflict, "Biển số" + data.PhuongTien.BienSo1 + "đã , vui lòng kiểm tra lại");
                // else if (uow.KeHoachBaoDuongs.Exists(x => x.PhuongTien.BienSo1 == data.PhuongTien.BienSo1 && x.IsDeleted))
                // {
                //     var t = uow.KeHoachBaoDuongs.GetAll(x => x.PhuongTien.BienSo1 == data.PhuongTien.BienSo1).FirstOrDefault();
                //     t.IsDeleted = false;
                //     t.DeletedBy = null;
                //     t.DeletedDate = null;
                //     t.UpdatedBy = Guid.Parse(User.Identity.Name);
                //     t.UpdatedDate = DateTime.Now;
                //     t.BaoDuong_Id = data.BaoDuong_Id;
                //     t.PhuongTien_Id = data.PhuongTien_Id;

                //     uow.KeHoachBaoDuongs.Update(t);

                // }
                // else
                // {
                LichSuBaoDuong dv = new LichSuBaoDuong();
                Guid id = Guid.NewGuid();
                dv.Id = id;
                dv.DiaDiem_Id = data.DiaDiem_Id;
                dv.CreatedDate = DateTime.Now;
                dv.CreatedBy = Guid.Parse(User.Identity.Name);
                dv.NguoiXacNhanHoanThanh_Id = Guid.Parse(User.Identity.Name);
                dv.NgayHoanThanh = data.NgayHoanThanh;
                dv.IsYeuCau = true;
                dv.IsDuyet = true;
                dv.IsBaoDuong = true;
                dv.IsHoanThanh = true;
                dv.BaoDuong_Id = data.BaoDuong_Id;
                dv.PhuongTien_Id = data.PhuongTien_Id;
                dv.NguoiXacNhanHoanThanh = data.NguoiXacNhanHoanThanh;
                dv.NoiDung = data.NoiDung;
                dv.KetQua = data.KetQua;
                dv.ChiPhi = data.ChiPhi;

                uow.LichSuBaoDuongs.Add(dv);
                // }

                uow.Complete();
                return Ok();
            }
        }

        [HttpPost("XuatFile")]
        public ActionResult ExportFileExcel_KeHoachGiaoXe_Tau(List<ImportMMS_LichSu> data)
        {
            string fullFilePath = Path.Combine(Directory.GetParent(environment.ContentRootPath).FullName, "Uploads/Templates/BaoCaoBaoDuong.xlsx");
            using (ExcelPackage package = new ExcelPackage(new FileInfo(fullFilePath)))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    package.Workbook.Worksheets.Add("Sheet1");
                }
                ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                int stt = 1;
                int rowIndex = 2;
                foreach (var item in data)
                {
                    int colIndex = 1;
                    string NgayDiBaoDuong = DateTime.TryParse(item.NgayBaoDuong, out DateTime parsedNgayDenKV)
                    ? parsedNgayDenKV.ToString("dd/MM/yyyy HH:mm") : item.NgayBaoDuong;
                    string NgayHoanThanh = DateTime.TryParse(item.NgayHoanThanh, out DateTime parsedNgayGiao)
                    ? parsedNgayGiao.ToString("dd/MM/yyyy HH:mm") : item.NgayHoanThanh;

                    worksheet.Cells[rowIndex, colIndex++].Value = stt;
                    worksheet.Cells[rowIndex, colIndex++].Value = item.BienSo1;
                    worksheet.Cells[rowIndex, colIndex++].Value = item.BienSo2;
                    worksheet.Cells[rowIndex, colIndex++].Value = NgayDiBaoDuong;
                    worksheet.Cells[rowIndex, colIndex++].Value = NgayHoanThanh;
                    worksheet.Cells[rowIndex, colIndex++].Value = item.DiaDiem;
                    worksheet.Cells[rowIndex, colIndex++].Value = item.ChiPhi;
                    using (var range = worksheet.Cells[rowIndex, 1, rowIndex, 11]) // Áp dụng từ cột 1 đến cột 3
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 251, 213));
                        ApplyBorder(range);
                    }

                    void ApplyBorder(ExcelRange range)
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                        range.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    }

                    for (int col = 1; col <= 11; col++)
                    {
                        var cell = worksheet.Cells[rowIndex, col];
                        var border = cell.Style.Border;
                        border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        if (col > 5 || col < 11)
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        else
                        {
                            cell.Style.WrapText = true;
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        }
                    }
                    if (item.lichSu != null && item.lichSu.Any())
                    {
                        rowIndex++;
                        foreach (var cx in item.lichSu)
                        {
                            colIndex = 8; // Bắt đầu ghi từ cột thứ 3 để lùi vào so với dòng cha
                            worksheet.Cells[rowIndex, colIndex++].Value = cx.NoiDungBaoDuong;
                            worksheet.Cells[rowIndex, colIndex++].Value = cx.DinhMuc;
                            worksheet.Cells[rowIndex, colIndex++].Value = cx.LoaiBaoDuong;
                            worksheet.Cells[rowIndex, colIndex++].Value = cx.GhiChu;
                            worksheet.Cells[rowIndex, colIndex++].Value = cx.TongChiPhi;
                            // Kẻ viền cho từng hàng chuyến xe
                            // using (var range = worksheet.Cells[rowIndex, 3, rowIndex, colIndex - 1])
                            using (var range = worksheet.Cells[rowIndex, 1, rowIndex, 11])
                            {
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 255, 255));
                                ApplyBorder(range);
                            }
                            rowIndex++;
                        }
                    }
                    else
                    {
                        rowIndex++; // Nếu không có chuyến xe, chỉ xuống dòng tiếp theo
                    }

                    ++stt;

                }
                return Ok(new { dataexcel = package.GetAsByteArray() });
            }
        }

        [HttpPut]
        public ActionResult Put(Guid id, LichSuBaoDuong duLieu)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (id != duLieu.Id)
                {
                    return BadRequest();
                }

                // Sao chép giá trị của CreatedDate và CreatedBy từ bản ghi hiện tại.
                duLieu.CreatedDate = DateTime.Now;
                // Cập nhật các trường còn lại
                duLieu.UpdatedBy = Guid.Parse(User.Identity.Name);
                duLieu.UpdatedDate = DateTime.Now;
                uow.LichSuBaoDuongs.Update(duLieu);
                uow.Complete();
                //Ghi log truy cập
                return StatusCode(StatusCodes.Status204NoContent);
            }
        }

        [HttpDelete]
        public ActionResult Delete(Guid id)
        {
            lock (Commons.LockObjectState)
            {
                LichSuBaoDuong duLieu = uow.LichSuBaoDuongs.GetById(id);

                if (duLieu == null)
                {
                    return NotFound();
                }
                duLieu.DeletedDate = DateTime.Now;
                duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                duLieu.IsDeleted = true;
                uow.LichSuBaoDuongs.Update(duLieu);
                uow.Complete();
                return Ok(duLieu);
            }

        }
    }
}
