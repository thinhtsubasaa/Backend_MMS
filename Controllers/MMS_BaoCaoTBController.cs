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

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MMS_BaoCaoTBController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        private readonly DataService _master;
        public MMS_BaoCaoTBController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment, DataService master)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
            _master = master;
        }

        [HttpGet("LichSuBaoDuongTheoTB")]
        public async Task<ActionResult> GetLichSuBaoDuongBienSo(Guid? ThietBi_Id, string keyword = null)
        {
            var dataList = await _master.GetDiadiem();
            Expression<Func<LichSuBaoDuong, bool>> whereFunc = item => !item.IsDeleted
            && item.ThietBi_Id != null
            && item.ThietBi_Id == ThietBi_Id
            && (string.IsNullOrEmpty(keyword)
            || item.ThietBi.MaThietBi.ToLower().Replace("-", "").Replace(".", "").Contains(keyword.ToLower().Replace("-", "").Replace(".", ""))
            || item.ThietBi.Name.ToLower().Replace("-", "").Replace(".", "").Contains(keyword.ToLower().Replace("-", "").Replace(".", "")));
            string[] includes = { "ThietBi" };
            var data = uow.LichSuBaoDuongs.GetAll(whereFunc, x => x.OrderByDescending(x => x.CreatedDate), includes)
             .Select(x => new
             {
                 x.Id,
                 x.DiaDiem_Id,
                 x.ThietBi?.Name,
                 TenDiaDiem = dataList.Where(t => !x.IsDeleted && t.Id == x.DiaDiem_Id)?.FirstOrDefault()?.TenDiaDiem,
                 Ngay = string.Format("{0:dd/MM/yyyy HH:mm}", x.Ngay),
                 NgayXacNhan = string.Format("{0:dd/MM/yyyy HH:mm}", x.NgayXacNhan),
                 NgayDiBaoDuong = string.Format("{0:dd/MM/yyyy HH:mm}", x.NgayDiBaoDuong),
                 NgayHoanThanh = string.Format("{0:dd/MM/yyyy HH:mm} ", x.NgayHoanThanh),
                 x.BaoDuong_Id,
                 x.NguoiYeuCau,
                 x.NguoiXacNhan,
                 x.NguoiDiBaoDuong,
                 x.NguoiXacNhanHoanThanh,
                 x.NoiDung,
                 x.KetQua,
                 x.IsYeuCau,
                 x.IsDuyet,
                 x.IsBaoDuong,
                 x.IsHoanThanh,
                 TinhTrang = x.IsHoanThanh ? "Đã hoàn thành bảo dưỡng" :
                 (x.IsBaoDuong && x.IsYeuCau && x.IsDuyet && x.IsLenhHoanThanh && !x.IsHoanThanh) ? "Đã được đề xuất hoàn thành bảo dưỡng" :
                    (x.IsBaoDuong && x.IsYeuCau && x.IsDuyet && !x.IsLenhHoanThanh && !x.IsHoanThanh) ? "Đang bảo dưỡng" :
                    (x.IsYeuCau && !x.IsBaoDuong && !x.IsDuyet && !x.IsHoanThanh) ? "Đã được yêu cầu bảo dưỡng" :
                    (x.IsDuyet && x.IsYeuCau && !x.IsBaoDuong && !x.IsHoanThanh) ? "Đã được duyệt bảo dưỡng" :
                    "Đang hoạt động",
                 ChiPhi = x.ChiPhi.ToString(),
             });
            return Ok(data);
        }

        [HttpGet("LichSuBaoDuong")]
        public async Task<ActionResult> GetLichSuBaoDuong(string keyword = null)
        {
            var dataList = await _master.GetDiadiem();
            Expression<Func<LichSuBaoDuong, bool>> whereFunc = item => !item.IsDeleted
            && item.ThietBi_Id != null
            && (string.IsNullOrEmpty(keyword)
            || item.ThietBi.MaThietBi.ToLower().Replace("-", "").Replace(".", "").Contains(keyword.ToLower().Replace("-", "").Replace(".", ""))
              || item.ThietBi.Name.ToLower().Replace("-", "").Replace(".", "").Contains(keyword.ToLower().Replace("-", "").Replace(".", "")));
            string[] includes = { "BaoDuong", "ThietBi" };
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
                 x.ThietBi_Id,
                 x.ThietBi?.MaThietBi,
                 x.ThietBi?.Name,
                 ThoiGianSuDung = x.ThietBi?.ThoiGianSuDung.ToString(),
                 x.ThietBi?.ThoiGian_NgayBaoDuong,
                 ChiSo = x.BaoDuong?.GiaTri.ToString(),
                 x.NoiDung,
                 x.KetQua,
                 TinhTrang = x.IsHoanThanh ? "Đã hoàn thành bảo dưỡng" :
                   (x.IsBaoDuong && x.IsYeuCau && x.IsDuyet && x.IsLenhHoanThanh && !x.IsHoanThanh) ? "Đã được đề xuất hoàn thành bảo dưỡng" :
                    (x.IsBaoDuong && x.IsYeuCau && x.IsDuyet && !x.IsLenhHoanThanh && !x.IsHoanThanh) ? "Đang bảo dưỡng" :
                    (x.IsYeuCau && !x.IsBaoDuong && !x.IsDuyet && !x.IsHoanThanh) ? "Đã được yêu cầu bảo dưỡng" :
                    (x.IsDuyet && x.IsYeuCau && !x.IsBaoDuong && !x.IsHoanThanh) ? "Đã được duyệt bảo dưỡng" :
                    "Đang hoạt động",
                 ChiPhi = x.ChiPhi.ToString(),
             });
            return Ok(data);
        }

        [HttpGet("DanhSachYeuCau")]
        public async Task<ActionResult> Get(string keyword = null)
        {
            var dataList = await _master.GetDiadiem();
            Expression<Func<LichSuBaoDuong, bool>> whereFunc = item => !item.IsDeleted
            && item.ThietBi_Id != null
           && item.TrangThai == null
            && (string.IsNullOrEmpty(keyword)
             || item.ThietBi.MaThietBi.ToLower().Replace("-", "").Replace(".", "").Contains(keyword.ToLower().Replace("-", "").Replace(".", ""))
              || item.ThietBi.Name.ToLower().Replace("-", "").Replace(".", "").Contains(keyword.ToLower().Replace("-", "").Replace(".", "")));
            string[] includes = { "BaoDuong", "ThietBi" };

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
                 x.ThietBi_Id,
                 x.ThietBi?.MaThietBi,
                 x.ThietBi?.Name,
                 ThoiGianSuDung = x.ThietBi.ThoiGianSuDung.ToString(),
                 x.ThietBi.ThoiGian_NgayBaoDuong,
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
            string[] include = { "BaoDuong", "ThietBi" };
            var query = uow.LichSuBaoDuongs.GetAll(x => !x.IsDeleted
            && x.ThietBi_Id != null
            && x.TrangThai != null
            && x.CreatedBy == User_Id, x => x.OrderByDescending(p => p.UpdatedDate), include);
            var data = query
            .Select(x => new
            {
                x.Id,
                x.ThietBi?.MaThietBi,
                x.ThietBi?.Name,
                Model = x.BaoDuong?.Name,
                Model_Option = x.BaoDuong?.Option,
                NgayYeuCau = string.Format("{0:dd/MM/yyyy HH:mm}", x.Ngay),
                NgayXacNhan = string.Format("{0:dd/MM/yyyy HH:mm}", x.NgayXacNhan),
                ThoiGianSuDung = x.ThietBi.ThoiGianSuDung.ToString(),
                ThoiGian_NgayBaoDuong = x.ThietBi.ThoiGian_NgayBaoDuong.ToString(),
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
            string[] include = { "BaoDuong", "ThietBi" };
            var query = uow.LichSuBaoDuongs.GetAll(x => !x.IsDeleted
            && x.ThietBi_Id != null
                && x.CreatedBy == User_Id
                 && (string.IsNullOrEmpty(keyword)
            || x.ThietBi.MaThietBi.ToLower().Replace("-", "").Replace(".", "").Contains(keyword.ToLower().Replace("-", "").Replace(".", ""))
              || x.ThietBi.Name.ToLower().Replace("-", "").Replace(".", "").Contains(keyword.ToLower().Replace("-", "").Replace(".", "")))
            , x => x.OrderByDescending(p => p.CreatedDate), include);
            var data = query
            .Select(x => new
            {
                x.Id,
                x?.ThietBi?.Name,
                Model = x.BaoDuong?.Name,
                Model_Option = x.BaoDuong?.Option,
                NgayYeuCau = string.Format("{0:dd/MM/yyyy HH:mm}", x.Ngay),
                NgayXacNhan = string.Format("{0:dd/MM/yyyy HH:mm}", x.NgayXacNhan),
                ThoiGianSuDung = x.ThietBi.ThoiGianSuDung.ToString(),
                ThoiGian_NgayBaoDuong = x.ThietBi.ThoiGian_NgayBaoDuong.ToString(),
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
            && item.ThietBi_Id != null
            && item.NgayDiBaoDuong.Value.Date >= TuNgay && item.NgayDiBaoDuong.Value.Date <= DenNgay
            && item.IsBaoDuong && !item.IsHoanThanh
            && (string.IsNullOrEmpty(keyword)
            || item.ThietBi.MaThietBi.ToLower().Replace("-", "").Replace(".", "").Contains(keyword.ToLower().Replace("-", "").Replace(".", ""))
            || item.ThietBi.Name.ToLower().Replace("-", "").Replace(".", "").Contains(keyword.ToLower().Replace("-", "").Replace(".", "")));
            string[] includes = { "BaoDuong", "ThietBi" };
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
                 x.ThietBi_Id,
                 ThoiGianSuDung = x.ThietBi.ThoiGianSuDung,
                 ThoiGian_NgayBaoDuong = x.ThietBi.ThoiGian_NgayBaoDuong,
                 ChiSo = x.BaoDuong?.GiaTri,
                 x.NoiDung,
                 x.KetQua,
                 x.ChiPhi,
             });
            return Ok(data);
        }

        [HttpGet("BaoDuong")]
        public async Task<ActionResult> GetKH(DateTime TuNgay, DateTime DenNgay, string keyword = null)
        {
            var dataList = await _master.GetDiadiem();
            Expression<Func<LichSuBaoDuong, bool>> whereFunc = item => !item.IsDeleted
            && item.ThietBi_Id != null
            && item.NgayHoanThanh.Value.Date >= TuNgay && item.NgayHoanThanh.Value.Date <= DenNgay
            && item.IsHoanThanh
            && (string.IsNullOrEmpty(keyword)
            || item.ThietBi.MaThietBi.ToLower().Replace("-", "").Replace(".", "").Contains(keyword.ToLower().Replace("-", "").Replace(".", ""))
            || item.ThietBi.Name.ToLower().Replace("-", "").Replace(".", "").Contains(keyword.ToLower().Replace("-", "").Replace(".", "")));
            string[] includes = { "BaoDuong", "ThietBi" };
            var data = uow.LichSuBaoDuongs.GetAll(whereFunc, null, includes)
             .Select(x => new
             {
                 x.Id,
                 x.DiaDiem_Id,
                 TenDiaDiem = dataList.Where(t => !x.IsDeleted && t.Id == x.DiaDiem_Id)?.FirstOrDefault()?.TenDiaDiem,
                 Ngay = string.Format("{0:dd/MM/yyyy}", x.Ngay),
                 NgayXacNhan = string.Format("{0:dd/MM/yyyy}", x.NgayXacNhan),
                 NgayDiBaoDuong = string.Format("{0:dd/MM/yyyy}", x.NgayDiBaoDuong),
                 NgayHoanThanh = string.Format("{0:dd/MM/yyyy} ", x.NgayHoanThanh),
                 x.BaoDuong_Id,
                 x.BaoDuong?.TanSuat,
                 x.BaoDuong?.GiaTri,
                 LoaiBaoDuong = x.BaoDuong?.Name,
                 x.ThietBi_Id,
                 ThoiGianSuDung = x.ThietBi.ThoiGianSuDung,
                 ThoiGian_NgayBaoDuong = x.ThietBi.ThoiGian_NgayBaoDuong,
                 ChiSo = x.BaoDuong?.GiaTri,
                 x.NoiDung,
                 x.KetQua,
                 x.ChiPhi,
             });
            return Ok(data);
        }

        [HttpGet("GetById")]
        public async Task<ActionResult> GetEdit(Guid? Id)
        {
            var dataList = await _master.GetDiadiem();
            Expression<Func<LichSuBaoDuong, bool>> whereFunc = item => !item.IsDeleted && item.Id == Id;
            string[] includes = { "BaoDuong", "ThietBi" };
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
                 x.ThietBi_Id,
                 ThoiGianSuDung = x.ThietBi.ThoiGianSuDung,
                 ThoiGian_NgayBaoDuong = x.ThietBi.ThoiGian_NgayBaoDuong,
                 x.ThietBi?.Name,
                 ChiSo = x.BaoDuong?.GiaTri,
                 x.NoiDung,
                 x.KetQua,
                 x.ChiPhi,
             }).FirstOrDefault();
            return Ok(data);
        }

        [HttpPost("YeuCauBaoDuong_QuanLy")]
        public ActionResult YeuCau([FromBody] List<LichSuBaoDuong> dataList, string TenNhanVien, string MaNhanVien, Guid? User_Id, string NgayDiBaoDuong)
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
                    duLieu.ThietBi_Id = data.ThietBi_Id;
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
                    var thietbi = uow.DS_Thietbis.GetById(data.ThietBi_Id);
                    if (thietbi == null)
                    {
                        return BadRequest("Không tìm thấy thiết bị ");
                    }
                    thietbi.LichSuBaoDuong_Id = duLieu.Id;
                    thietbi.TinhTrang_Id = Guid.Parse("3fcfbc57-b53e-4226-8251-674c5a839b60");
                    uow.DS_Thietbis.Update(thietbi);
                    uow.Complete();
                }
                //Ghi log truy cập
                return Ok(1);
            }
        }

        [HttpPost("YeuCauBaoDuong")]
        public ActionResult LSYeuCauThayDoiKHDiGap([FromBody] List<LichSuBaoDuong> dataList, string TenNhanVien, string MaNhanVien, Guid? User_Id)
        {
            lock (Commons.LockObjectState)
            {
                var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
                foreach (var data in dataList)
                {
                    LichSuBaoDuong duLieu = new LichSuBaoDuong();
                    duLieu.Id = Guid.NewGuid();
                    duLieu.ThietBi_Id = data.ThietBi_Id;
                    duLieu.BaoDuong_Id = data.BaoDuong_Id;
                    duLieu.SoKM = data.SoKM;
                    duLieu.NguoiYeuCau_Id = User_Id;
                    duLieu.Ngay = DateTime.Now;
                    duLieu.IsYeuCau = true;
                    duLieu.NguoiYeuCau = TenNhanVien + "-" + MaNhanVien;
                    duLieu.CreatedDate = DateTime.Now;
                    duLieu.CreatedBy = Guid.Parse(User.Identity.Name);
                    uow.LichSuBaoDuongs.Add(duLieu);

                    var thietbi = uow.DS_Thietbis.GetById(data.ThietBi_Id);
                    if (thietbi == null)
                    {
                        return BadRequest("Không tìm thấy thiết bị ");
                    }
                    thietbi.LichSuBaoDuong_Id = duLieu.Id;
                    thietbi.TinhTrang_Id = Guid.Parse("d85552ee-7f15-4626-870b-ea8329afb6dd");
                    uow.DS_Thietbis.Update(thietbi);

                    uow.Complete();
                }
                //Ghi log truy cập
                return Ok(1);
            }
        }

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
                    var thietbi = uow.DS_Thietbis.GetById(yeuCau.ThietBi_Id);
                    if (thietbi == null)
                    {
                        return BadRequest("Không tìm thấy thiết bị ");
                    }

                    thietbi.TinhTrang_Id = Guid.Parse("90b5fae3-8fb6-43e3-8af2-6cec3627d3fc");
                    uow.DS_Thietbis.Update(thietbi);

                    uow.Complete();

                }
                //Ghi log truy cập
                return Ok(1);
            }
        }

        [HttpPost("HuyYeuCauBaoDuong_Wed")]
        public ActionResult HuyYeuCauWed(Guid? id, string TenNhanVien, string MaNhanVien, Guid? User_Id, string LiDo)
        {
            lock (Commons.LockObjectState)
            {
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
                var thietbi = uow.DS_Thietbis.GetById(yeuCau.ThietBi_Id);
                if (thietbi == null)
                {
                    return BadRequest("Không tìm thấy thiết bị ");
                }

                thietbi.TinhTrang_Id = Guid.Parse("90b5fae3-8fb6-43e3-8af2-6cec3627d3fc");
                uow.DS_Thietbis.Update(thietbi);

                uow.Complete();

                //Ghi log truy cập
                return Ok(1);
            }
        }

        [HttpPost("XacNhanYeuCau")]
        public ActionResult XacNhanYeuCauThayDoiKH([FromBody] List<LichSuBaoDuong> dataList, string NgayDiBaoDuong, string TenNhanVien, string MaNhanVien, Guid? User_Id)
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
                    var thietbi = uow.DS_Thietbis.GetById(yeuCau.ThietBi_Id);
                    if (thietbi == null)
                    {
                        return BadRequest("Không tìm thấy thiết bị ");
                    }

                    thietbi.TinhTrang_Id = Guid.Parse("3fcfbc57-b53e-4226-8251-674c5a839b60");
                    uow.DS_Thietbis.Update(thietbi);


                    uow.Complete();
                }
                //Ghi log truy cập
                return Ok(1);
            }
        }
        private DateTime? TryParseDate(string dateStr)
        {
            return DateTime.TryParseExact(dateStr, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)
                ? date
                : null;
        }

        [HttpPost("XacNhanYeuCau_Wed")]
        public ActionResult XacNhanYeuCauWed(Guid? id, Guid? DiaDiem_Id, string NgayDiBaoDuong, string TenNhanVien, string MaNhanVien, Guid? User_Id)
        {
            lock (Commons.LockObjectState)
            {
                // var yeuCau = uow.LichSuThayDoiKeHoachs.GetAll(x => !x.IsDeleted && x.KeHoachGiaoXe_Id == data.KeHoachGiaoXe_Id && x.TrangThai == null).OrderByDescending(x => x.ThoiGianYC).FirstOrDefault();
                var yeuCau = uow.LichSuBaoDuongs.GetById(id);
                if (yeuCau == null)
                {
                    return BadRequest($"Yêu cầu không tồn tại ");
                }
                var thietbi = uow.DS_Thietbis.GetById(yeuCau.ThietBi_Id);
                if (thietbi == null)
                {
                    return BadRequest("Không tìm thấy thiết bị ");
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
                    thietbi.TinhTrang_Id = Guid.Parse("3fcfbc57-b53e-4226-8251-674c5a839b60");
                }
                else
                {
                    yeuCau.NguoiHuyDuyet = TenNhanVien + "-" + MaNhanVien;
                    yeuCau.NguoiHuyDuyet_Id = Guid.Parse(User.Identity.Name);
                    thietbi.TinhTrang_Id = Guid.Parse("d85552ee-7f15-4626-870b-ea8329afb6dd");
                }

                yeuCau.UpdatedDate = DateTime.Now;
                yeuCau.UpdatedBy = Guid.Parse(User.Identity.Name);

                uow.LichSuBaoDuongs.Update(yeuCau);
                uow.DS_Thietbis.Update(thietbi);
                uow.Complete();

                //Ghi log truy cập
                return Ok(1);
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
                var thietbi = uow.DS_Thietbis.GetById(yeuCau.ThietBi_Id);
                if (thietbi == null)
                {
                    return BadRequest("Không tìm thấy thiết bị ");
                }

                thietbi.TinhTrang_Id = Guid.Parse("d7ed914c-425a-4909-59c3-08dd29516aa7");
                uow.DS_Thietbis.Update(thietbi);


                uow.Complete();

                //Ghi log truy cập
                return Ok(1);
            }
        }
        [HttpPost("HoanThanhBaoDuong")]
        public ActionResult XacNhanHoanThanhBaoDuong(LichSuBaoDuong data, string TenNhanVien, string MaNhanVien, Guid? User_Id, int ThoiGian, string File, int ChiPhi)
        {
            lock (Commons.LockObjectState)
            {
                // var exit = userManager.FindByIdAsync(User.Identity.Name).Result;

                var yeuCau = uow.LichSuBaoDuongs.GetById(data.Id);
                if (yeuCau == null)
                {
                    return BadRequest($"Yêu cầu không tồn tại ");
                }

                yeuCau.NgayHoanThanh = DateTime.Now;
                yeuCau.NguoiXacNhanHoanThanh_Id = User_Id;
                yeuCau.IsHoanThanh = true;
                yeuCau.NoiDung = data.NoiDung;
                yeuCau.KetQua = data.KetQua;
                yeuCau.NguoiXacNhanHoanThanh = TenNhanVien + "-" + MaNhanVien;
                yeuCau.HinhAnh = File;
                yeuCau.ChiPhi = ChiPhi;
                yeuCau.ThoiGianSuDung = ThoiGian;
                yeuCau.UpdatedDate = DateTime.Now;
                yeuCau.UpdatedBy = Guid.Parse(User.Identity.Name);
                uow.LichSuBaoDuongs.Update(yeuCau);

                var thietbi = uow.DS_Thietbis.GetById(data.ThietBi_Id);
                if (thietbi == null)
                {
                    return BadRequest("Không tìm thấy thiết bị ");
                }
                else
                {
                    thietbi.UpdatedBy = Guid.Parse(User.Identity.Name);
                    thietbi.UpdatedDate = DateTime.Now;
                    thietbi.ThoiGian_NgayBaoDuong = ThoiGian;
                    thietbi.TinhTrang_Id = Guid.Parse("65f0a161-31bd-4ae6-b487-e3c95fea3dc4");
                    uow.DS_Thietbis.Update(thietbi);
                }
                uow.Complete();

                //Ghi log truy cập
                return Ok(1);
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
                var thietbi = uow.DS_Thietbis.GetAll(x => !x.IsDeleted);
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
                        object MaThietBi = worksheet.Cells[i, 2].Value;
                        object TenThietBi = worksheet.Cells[i, 3].Value;
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
                        info.MaThietBi = MaThietBi?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.TenThietBi = TenThietBi?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";

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

                        if (string.IsNullOrEmpty(info.MaThietBi))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Thiết bị không được để trống");
                        }
                        else
                        {
                            var info_thietbi = thietbi.Where(x => x.MaThietBi == info.MaThietBi.ToUpper() || x.Name == info.TenThietBi.ToUpper()).FirstOrDefault();
                            if (info_thietbi == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo thiết bị trong danh sách Thiết bị");
                            }
                            else
                            {
                                info.ThietBi_Id = info_thietbi.Id;

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

                    var exit = uow.LichSuBaoDuongs.GetSingle(x => !x.IsDeleted && x.ThietBi_Id != null && x.ThietBi.MaThietBi == item.MaThietBi.ToUpper());
                    if (exit == null)
                    {
                        uow.LichSuBaoDuongs.Add(new LichSuBaoDuong
                        {
                            ThietBi_Id = item.ThietBi_Id,
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
                        exit.ThietBi_Id = item.ThietBi_Id;
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
                dv.ThietBi_Id = data.ThietBi_Id;
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
