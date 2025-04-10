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
using System.Threading.Tasks;
using ERP.Helpers;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MMS_ThongTinTheoHangMucController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        private readonly DataService _master;
        public MMS_ThongTinTheoHangMucController(IUnitofWork _uow, DataService master, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
            _master = master;
        }


        [HttpGet()]
        public async Task<ActionResult> GetAsync(string keyword = null)
        {
            var dataDonVi = await _master.GetDonVi();
            string[] includes = { "PhuongTien", "HangMuc", "PhuongTien.DM_Model", "PhuongTien.DM_Loai", "PhuongTien.LichSuBaoDuong", "PhuongTien.PhuTrachBoPhan" };
            var data = uow.ThongTinTheoHangMucs.GetAll(t => !t.IsDeleted
            && t.HangMuc.GhiChu != "Kiểm tra thường xuyên"
            && (string.IsNullOrEmpty(keyword) || t.PhuongTien.BienSo1.ToLower().Contains(keyword.ToLower())
            || t.PhuongTien.BienSo2.ToLower().Contains(keyword.ToLower()))
            && (((t.HangMuc.DinhMuc - (t.PhuongTien.SoKM - t.GiaTriBaoDuong)) <= t.HangMuc.CanhBao_GanDenHan)
            || ((t.HangMuc.DinhMuc - (t.PhuongTien.SoKM_Adsun - t.GiaTriBaoDuong)) <= t.HangMuc.CanhBao_GanDenHan))
            , null, includes).Select(x => new
            {
                x.Id,
                x.PhuongTien?.BienSo1,
                x.PhuongTien?.BienSo2,
                x.PhuongTien?.MaPhuongTien,
                Model = x.PhuongTien?.DM_Model?.Name,
                x.PhuongTien?.DM_Model?.Option,
                LoaiPT = x.PhuongTien?.DM_Loai?.Name,
                x.PhuongTien?.PhuTrachBoPhan?.NhanVien,
                NgayHoanThanh = string.Format("{0:dd/MM/yyyy HH:mm} ", x.PhuongTien?.LichSuBaoDuong?.NgayDeXuatHoanThanh),
                SoNgaySuDung = x.PhuongTien?.LichSuBaoDuong?.NgayDeXuatHoanThanh != null ? (DateTime.Now - x.PhuongTien?.LichSuBaoDuong?.NgayDeXuatHoanThanh)?.Days : null,
                DonViSuDung = dataDonVi.Where(t => !t.IsDeleted && t.Id == x.PhuongTien?.DonVi_Id)?.FirstOrDefault()?.TenDonVi,
                x.HangMuc?.NoiDungBaoDuong,
                x.PhuongTien.DonVi_Id,
                // x.HangMuc?.DinhMuc,
                DinhMuc = string.Format("{0:N0}", x.HangMuc?.DinhMuc),
                x.PhuongTien_Id,
                x.PhuongTien?.SoKM,
                x.GiaTriBaoDuong,
                GiaTriVoiNgayBaoDuong = x.PhuongTien?.SoKM - x.GiaTriBaoDuong,
                x.HangMuc_Id,
                IsYeuCau = x.PhuongTien.LichSuBaoDuong_Id != null && !x.PhuongTien.LichSuBaoDuong.IsHoanThanh ? x.PhuongTien.LichSuBaoDuong.IsYeuCau : false,
            }).ToList();
            var groupedData = data
      .GroupBy(x => x.PhuongTien_Id)
      .Select(group => new
      {
          PhuongTien_Id = group.Key,
          BienSo1 = group.FirstOrDefault()?.BienSo1,
          BienSo2 = group.FirstOrDefault()?.BienSo2,
          SoKM = group.FirstOrDefault()?.SoKM,
          group.FirstOrDefault()?.Model,
          group.FirstOrDefault()?.Option,
          group.FirstOrDefault()?.LoaiPT,
          group.FirstOrDefault()?.DonViSuDung,
          group.FirstOrDefault()?.GiaTriBaoDuong,
          group.FirstOrDefault()?.GiaTriVoiNgayBaoDuong,
          group.FirstOrDefault()?.IsYeuCau,
          group.FirstOrDefault()?.DonVi_Id,
          group.FirstOrDefault()?.MaPhuongTien,
          group.FirstOrDefault()?.NgayHoanThanh,
          group.FirstOrDefault()?.SoNgaySuDung,
          NguoiPhuTrach = group.FirstOrDefault()?.NhanVien,
          HangMucs = group.Select(hm => new
          {
              hm.HangMuc_Id,
              hm.NoiDungBaoDuong,
              hm.DinhMuc,
              hm.GiaTriBaoDuong,

          }).ToList()
      });
            return Ok(groupedData);
        }

        [HttpGet("DSHangMuc")]
        public ActionResult GetHangMuc(Guid? Id_PhuongTien)
        {
            string[] includes = { "PhuongTien", "HangMuc", "HangMuc.DM_TanSuat" };
            var data = uow.ThongTinTheoHangMucs.GetAll(t => !t.IsDeleted
            && t.PhuongTien_Id == Id_PhuongTien
            && t.HangMuc.GhiChu != "Kiểm tra thường xuyên"
            , x => x.OrderByDescending(x => x.UpdatedDate), includes).Select(x => new
            {
                x.Id,
                x.PhuongTien?.BienSo1,
                x.PhuongTien?.BienSo2,

                x.HangMuc?.NoiDungBaoDuong,
                // x.HangMuc?.DinhMuc,
                DinhMuc = string.Format("{0:N0}", x.HangMuc?.DinhMuc),
                DinhMuc2 = string.Format("{0:N0}", x.HangMuc?.DinhMuc),
                x.PhuongTien_Id,
                SoKM = string.Format("{0:N0}", x.PhuongTien?.SoKM),
                TieuChi = x.HangMuc.DM_TanSuat?.TanSuat,
                x.GiaTriBaoDuong,
                x.HangMuc?.LoaiBaoDuong,
                x.PhuongTien?.SoKM_NgayBaoDuong,
                x.HangMuc_Id,
                x.PhuongTien?.LichSuBaoDuong_Id,
                x.HangMuc?.GhiChu,
                TongChiPhi_TD = string.Format("{0:N0}", x.TongChiPhi_TD),
                TongChiPhi2 = x.TongChiPhi_TD.ToString(),
                ketqua = x.HangMuc.DinhMuc - (x.PhuongTien.SoKM - x.GiaTriBaoDuong),
                IsDenHan = (x.HangMuc.DinhMuc - (x.PhuongTien.SoKM - x.GiaTriBaoDuong)) <= x.HangMuc.CanhBao_GanDenHan,
                IsDaDenHan = (x.HangMuc.DinhMuc - (x.PhuongTien.SoKM - x.GiaTriBaoDuong)) <= x.HangMuc.CanhBao_DenHan,

            }).OrderByDescending(x => x.IsDenHan);
            return Ok(data);
        }

        [HttpGet("DSHangMucYeuCau")]
        public ActionResult GetYeuCau(Guid? Id_PhuongTien)
        {
            var chitiet = uow.LichSuBaoDuong_ChiTiets.GetAll(x => !x.IsDeleted && !x.LichSuBaoDuong.IsHoanThanh && x.LichSuBaoDuong.TrangThai == null && x.PhuongTien_Id == Id_PhuongTien);
            string[] includes = { "PhuongTien", "HangMuc", "HangMuc.DM_TanSuat" };
            var data = uow.ThongTinTheoHangMucs.GetAll(t => !t.IsDeleted
            && t.PhuongTien_Id == Id_PhuongTien
            && t.HangMuc.GhiChu != "Kiểm tra thường xuyên"
            , x => x.OrderByDescending(x => x.UpdatedDate), includes).Select(x => new
            {
                x.Id,
                x.PhuongTien?.BienSo1,
                x.PhuongTien?.BienSo2,
                x.HangMuc?.NoiDungBaoDuong,
                // x.HangMuc?.DinhMuc,
                DinhMuc = string.Format("{0:N0}", x.HangMuc?.DinhMuc),
                DinhMuc2 = string.Format("{0:N0}", x.HangMuc?.DinhMuc),
                x.PhuongTien_Id,
                SoKM = string.Format("{0:N0}", x.PhuongTien?.SoKM),
                TieuChi = x.HangMuc.DM_TanSuat?.TanSuat,
                x.GiaTriBaoDuong,
                x.HangMuc?.LoaiBaoDuong,
                x.PhuongTien?.SoKM_NgayBaoDuong,
                x.HangMuc_Id,
                x.PhuongTien?.LichSuBaoDuong_Id,
                x.HangMuc?.GhiChu,
                TongChiPhi_TD = string.Format("{0:N0}", x.TongChiPhi_TD),
                TongChiPhi2 = x.TongChiPhi_TD.ToString(),
                IsDenHan = chitiet.Any(t => t.HangMuc_Id == x.HangMuc_Id),
                IsGanDenHan = (x.HangMuc.DinhMuc - (x.PhuongTien.SoKM - x.GiaTriBaoDuong)) <= x.HangMuc.CanhBao_GanDenHan,
                IsDaDenHan = (x.HangMuc.DinhMuc - (x.PhuongTien.SoKM - x.GiaTriBaoDuong)) <= x.HangMuc.CanhBao_DenHan,

            }).OrderByDescending(x => x.IsDenHan);
            return Ok(data);
        }



        [HttpGet("DSTheoId")]
        public ActionResult Get(Guid? Id_PhuongTien)
        {
            string[] includes = { "PhuongTien", "HangMuc", };
            var data = uow.ThongTinTheoHangMucs.GetAll(t => !t.IsDeleted
            && t.PhuongTien_Id == Id_PhuongTien
            && t.HangMuc.GhiChu != "Kiểm tra thường xuyên"
            && (((t.HangMuc.DinhMuc - (t.PhuongTien.SoKM - t.GiaTriBaoDuong)) <= t.HangMuc.CanhBao_GanDenHan)
            || ((t.HangMuc.DinhMuc - (t.PhuongTien.SoKM_Adsun - t.GiaTriBaoDuong)) <= t.HangMuc.CanhBao_GanDenHan)),
             null, includes).Select(x => new
             {
                 x.Id,
                 x.PhuongTien?.BienSo1,
                 x.PhuongTien?.BienSo2,
                 x.HangMuc?.NoiDungBaoDuong,
                 x.HangMuc?.DinhMuc,
                 x.PhuongTien_Id,
                 x.PhuongTien?.SoKM,
                 x.GiaTriBaoDuong,
                 x.HangMuc?.LoaiBaoDuong,
                 x.HangMuc_Id,
                 x.HangMuc?.GhiChu,
             });
            return Ok(data);
        }


        [HttpPost("Read_Excel")]
        public ActionResult Read_Excel(IFormFile file)
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            DateTime dt = DateTime.Now;
            // Rename file
            string fileName = (long)timeSpan.TotalSeconds + "_" + Commons.TiengVietKhongDau(file.FileName);
            string fileExt = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
            string[] supportedTypes = new[] { "xls", "xlsx" };
            if (supportedTypes.Contains(fileExt))
            {
                var Nhom = uow.DM_Nhoms.GetAll(x => !x.IsDeleted);
                string webRootPath = environment.WebRootPath;
                if (string.IsNullOrWhiteSpace(webRootPath))
                {
                    webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                }
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
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[9];
                    int rowCount = worksheet.Dimension.Rows;
                    var list_datas = new List<ImportMMS_DM_Loai>();
                    for (int i = 2; i <= rowCount; i++)
                    {
                        if (worksheet.Cells[i, 3].Value == null)
                        {
                            // Nếu không có dữ liệu, dừng vòng lặp
                            break;
                        }

                        object Name = worksheet.Cells[i, 2].Value;
                        object Code = worksheet.Cells[i, 3].Value;
                        object ThuocNhom = worksheet.Cells[i, 5].Value;
                        object Note = worksheet.Cells[i, 6].Value;

                        DateTime baseDate = new DateTime(1900, 1, 1);
                        int excelDate = 45329;
                        var lst_Lois = new List<string>();
                        var info = new ImportMMS_DM_Loai();
                        info.Id = Guid.NewGuid();
                        info.Name = Name?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.Code = Code?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.ThuocNhom = ThuocNhom?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.Note = Note?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";

                        if (string.IsNullOrEmpty(info.Name))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Mã phương tiện không được để trống");
                        }
                        if (string.IsNullOrEmpty(info.ThuocNhom))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Nhóm không được để trống");
                        }
                        else
                        {
                            var info_Nhom = Nhom.Where(x => x.Name.ToLower() == info.ThuocNhom.ToLower()).FirstOrDefault();
                            if (info_Nhom == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo nhóm trong danh mục Nhóm");
                            }
                            else
                            {
                                info.Nhom_Id = info_Nhom.Id;

                            }

                        }
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
                            if (!string.IsNullOrEmpty(item.Name))
                            {
                                if (DuplicateSoKhungs(list_datas.Where(x => x.Name == item.Name).ToList()) > 0)
                                {
                                    item.IsLoi = true;
                                    item.lst_Lois.Add($"Số Khung {item.Name} bị trùng lặp");
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
        private int DuplicateSoKhungs(List<ImportMMS_DM_Loai> lst)
        {
            return lst.GroupBy(p => new { p.Name }).Where(p => p.Count() > 1).Count();
        }

        [HttpPost("Save_Import")]
        public ActionResult Post_SaveImport(List<ImportMMS_DM_Loai> DH)
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

                    var exit = uow.DM_Loais.GetSingle(x => !x.IsDeleted && x.Code.ToLower() == item.Code.ToLower());
                    if (exit == null)
                    {
                        uow.DM_Loais.Add(new DM_Loai
                        {
                            Name = item.Name,
                            Note = item.Note,
                            ThuocNhom = item.ThuocNhom,
                            Code = item.Code,
                            Nhom_Id = item.Nhom_Id,
                            CreatedDate = DateTime.Now,
                            CreatedBy = Guid.Parse(User.Identity.Name),
                        });
                    }
                    else
                    {
                        exit.Code = item.Code;
                        exit.ThuocNhom = item.ThuocNhom;
                        exit.Name = item.Name;
                        exit.Note = item.Note;
                        exit.Nhom_Id = item.Nhom_Id;
                        exit.UpdatedDate = DateTime.Now;
                        exit.UpdatedBy = Guid.Parse(User.Identity.Name);
                        uow.DM_Loais.Update(exit);
                    }

                }
                uow.Complete();
                return StatusCode(StatusCodes.Status200OK);
            }
        }
        [HttpGet("FileMau")]
        public ActionResult FileMauTM_DB()
        {
            string fullFilePath = Path.Combine(Directory.GetParent(environment.ContentRootPath).FullName, "Uploads/Templates/FileMau_DM_Loai.xlsx");
            string fileName = "FileMau_DM_Loai_" + DateTime.Now.ToString("HHmmss") + ".xlsx";
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

        [HttpGet("GetById")]
        public ActionResult Get(Guid id)
        {
            string[] includes = { "DM_Nhom" };
            var query = uow.DM_Loais.GetAll(x => x.Id == id, null, includes).Select(x => new
            {
                x.Id,

                x.Name,
                x.Code,
                ThuocNhom = x.DM_Nhom?.Name,
                x.Nhom_Id,
            }).FirstOrDefault();
            if (query == null)
            {
                return NotFound();
            }
            return Ok(query);
        }

        [HttpPost]
        public ActionResult Post(DM_Loai data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (uow.DM_Loais.Exists(x => x.Name == data.Name && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.Name + " đã tồn tại trong hệ thống");
                else if (uow.DM_Loais.Exists(x => x.Name == data.Name && x.IsDeleted))
                {

                    var d = uow.DM_Loais.GetAll(x => x.Name == data.Name).FirstOrDefault();
                    d.IsDeleted = false;
                    d.DeletedBy = null;
                    d.DeletedDate = null;
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.Name = data.Name;
                    d.Code = data.Code;
                    d.ThuocNhom = data.ThuocNhom;
                    d.Nhom_Id = data.Nhom_Id;
                    uow.DM_Loais.Update(d);

                }
                else
                {
                    DM_Loai cv = new DM_Loai();
                    Guid id = Guid.NewGuid();
                    cv.Id = id;
                    cv.Name = data.Name;
                    cv.Code = data.Code;
                    cv.ThuocNhom = data.ThuocNhom;
                    cv.Nhom_Id = data.Nhom_Id;
                    cv.CreatedDate = DateTime.Now;
                    cv.CreatedBy = Guid.Parse(User.Identity.Name);
                    uow.DM_Loais.Add(cv);
                }

                uow.Complete();
                return Ok();
            }
        }

        [HttpPut]
        public ActionResult Put(Guid id, DM_Loai duLieu)
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
                duLieu.UpdatedBy = Guid.Parse(User.Identity.Name);
                duLieu.UpdatedDate = DateTime.Now;
                uow.DM_Loais.Update(duLieu);
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
                DM_Loai duLieu = uow.DM_Loais.GetById(id);

                if (duLieu == null)
                {
                    return NotFound();
                }
                duLieu.DeletedDate = DateTime.Now;
                duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                duLieu.IsDeleted = true;
                uow.DM_Loais.Update(duLieu);
                uow.Complete();
                return Ok(duLieu);
            }

        }

    }
}
