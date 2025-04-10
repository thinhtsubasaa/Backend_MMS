using ERP.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.Configuration;
using static ERP.Data.MyDbContext;
using ThacoLibs;
using System.Data;
using System;
using ERP.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Office2010.Excel;
using System.IO;
using ERP.Helpers;
using System.Threading.Tasks;
using OfficeOpenXml;
namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MMS_PhanBoDonViController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        private readonly DbAdapter dbAdapter;
        private readonly DataService _master;
        private readonly IConfiguration configuration;
        public MMS_PhanBoDonViController(IConfiguration _configuration, IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment, DataService master)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
            _master = master;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            dbAdapter = new DbAdapter(connectionString);
        }

        [HttpGet("PhuongTien")]
        public async Task<ActionResult> GetPTien(Guid? DonVi_Id, string keyword = null)
        {
            var dataDonVi = await _master.GetDonVi();
            var dataBoPhan = await _master.GetBoPhan();
            var phanbo = uow.LichSuPhanBoDonVis.GetAll(x => !x.IsDeleted, x => x.OrderByDescending(x => x.CreatedDate));

            var data = uow.DS_PhuongTiens.GetAll(t => !t.IsDeleted
             && (DonVi_Id == null || t.DonVi_Id == DonVi_Id)
            && (string.IsNullOrEmpty(keyword) || t.BienSo1.ToLower().Contains(keyword.ToLower())
            || t.BienSo2.ToLower().Contains(keyword.ToLower())), t => t.OrderByDescending(x => x.NgayBatDau)
                ).Select(x =>
                {
                    var ngayKetThuc = phanbo.Where(t => t.BienSo == x.BienSo1)?.FirstOrDefault()?.NgayKetThuc;
                    var UpdateDonVi_Id = (ngayKetThuc.HasValue && ngayKetThuc.Value < DateTime.Now) ? null : x.DonVi_Id;
                    var updateBoPhan_Id = UpdateDonVi_Id != null ? x.BoPhan_Id : null;
                    return new
                    {
                        x.Id,
                        x.BienSo1,
                        x.SoKhung,
                        x.Model_Id,
                        DonVi_Id = UpdateDonVi_Id,
                        BoPhan_Id = updateBoPhan_Id,
                        TenBoPhan = dataBoPhan.Where(t => !t.IsDeleted && t.Id == updateBoPhan_Id)?.FirstOrDefault()?.TenPhongBan,
                        x.BienSo2,
                        DonViSuDung = dataDonVi.Where(t => !t.IsDeleted && t.Id == UpdateDonVi_Id)?.FirstOrDefault()?.TenDonVi,
                        x.KLBT,
                        x.KLHH,
                        x.KLKT,
                        x.KLTB,
                        x.Note,
                        x.Address_nearest,
                        x.MaPhuongTien,
                        x.SoKM,
                        x.SoChuyenXe,
                        NgayBatDau = string.Format("{0:dd/MM/yyyy}", phanbo.Where(t => t.BienSo == x.BienSo1)?.FirstOrDefault()?.NgayBatDau),
                        NgayKetThuc = string.Format("{0:dd/MM/yyyy}", ngayKetThuc)
                    };
                })
                 .OrderBy(x => x.DonVi_Id.HasValue ? 1 : 0)
                 .ThenByDescending(x => x.NgayBatDau);

            return Ok(data);
        }


        [HttpGet("GetById")]
        public async Task<ActionResult> Get(Guid id)
        {
            var dataDonVi = await _master.GetDonVi();
            var phanbo = uow.LichSuPhanBoDonVis.GetAll(x => !x.IsDeleted);

            var data = uow.DS_PhuongTiens.GetAll(t => !t.IsDeleted
           , t => t.OrderByDescending(x => x.NgayBatDau)
                ).Select(x => new
                {
                    x.Id,
                    x.BienSo1,
                    x.SoKhung,
                    x.Model_Id,
                    x.DonVi_Id,
                    x.LoaiPT_Id,
                    x.BienSo2,
                    DonViSuDung = dataDonVi.Where(t => !t.IsDeleted && t.Id == x.DonVi_Id)?.FirstOrDefault()?.TenDonVi,
                    x.KLBT,
                    x.KLHH,
                    x.KLKT,
                    x.KLTB,
                    x.Note,
                    x.Address_nearest,
                    x.MaPhuongTien,
                    x.SoKM,
                    x.SoChuyenXe,
                    NgayBatDau = string.Format("{0:dd/MM/yyyy}", phanbo.Where(t => t.BienSo == x.BienSo1)?.FirstOrDefault()?.NgayBatDau),
                    NgayKetThuc = string.Format("{0:dd/MM/yyyy}", phanbo.Where(t => t.BienSo == x.BienSo1)?.FirstOrDefault()?.NgayKetThuc),

                });
            return Ok(data);
        }

        [HttpGet()]
        public async Task<ActionResult> GetAllTable(Guid? PhuongTien_Id, string keyword)
        {
            var dataDonVi = await _master.GetDonVi();
            var dataBoPhan = await _master.GetBoPhan();

            var query = uow.LichSuPhanBoDonVis.GetAll(x => !x.IsDeleted
            && (PhuongTien_Id == null || x.PhuongTien_Id == PhuongTien_Id)
            && (string.IsNullOrEmpty(keyword) || x.BienSo.ToLower().Contains(keyword.ToLower())), x => x.OrderByDescending(x => x.CreatedDate))
            .Select(x => new
            {
                key = x.Id,
                x.Id,
                x.BienSo,
                NgayBatDau = string.Format("{0:dd/MM/yyyy}", x.NgayBatDau),
                NgayKetThuc = string.Format("{0:dd/MM/yyyy}", x.NgayKetThuc),
                x.DonVi_Id,
                x.BoPhan_Id,
                TenBoPhan = dataBoPhan.Where(t => !t.IsDeleted && t.Id == x.BoPhan_Id)?.FirstOrDefault()?.TenPhongBan,
                TenDonVi = dataDonVi.Where(t => !t.IsDeleted && t.Id == x.DonVi_Id)?.FirstOrDefault()?.TenDonVi,
            });

            return Ok(query);
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
                var dataPT = uow.DS_PhuongTiens.GetAll(x => !x.IsDeleted).Select(t => new { t.Id, t.BienSo1 }).ToList();
                var dataList = await _master.GetDonVi();
                var dataBoPhan = await _master.GetBoPhan();

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
                    var list_datas = new List<ImportMMS_DS_PhanBoDonVi>();
                    for (int i = 1; i <= rowCount; i++)
                    {
                        if (worksheet.Cells[i, 3].Value == null)
                        {
                            // Nếu không có dữ liệu, dừng vòng lặp
                            break;
                        }
                        object BienSo1 = worksheet.Cells[i, 1].Value;
                        object DonViSuDung = worksheet.Cells[i, 2].Value;
                        object BoPhan = worksheet.Cells[i, 3].Value;
                        object NgayBatDau = worksheet.Cells[i, 4].Value;
                        object NgayKetThuc = worksheet.Cells[i, 5].Value;

                        DateTime baseDate = new DateTime(1900, 1, 1);
                        int excelDate = 45329;
                        var lst_Lois = new List<string>();
                        var info = new ImportMMS_DS_PhanBoDonVi();
                        info.Id = Guid.NewGuid();

                        info.BienSo1 = BienSo1?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.DonViSuDung = DonViSuDung?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.BoPhanSuDung = BoPhan?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
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
                        info.NgayBatDau = ProcessDateString(ngayBatDau, baseDate);
                        var ngayKetThuc = NgayKetThuc?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.NgayKetThuc = ProcessDateString(ngayKetThuc, baseDate);


                        if (string.IsNullOrEmpty(info.BienSo1))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Biển số không được để trống");
                        }
                        else
                        {
                            var info_BienSo = dataPT.Where(x => x.BienSo1.ToLower().Replace("-", "").Replace(".", "").Contains(info.BienSo1.ToLower().Replace("-", "").Replace(".", ""))).FirstOrDefault();
                            if (info_BienSo == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo biển số trong danh mục mục biển số");
                            }
                            else
                            {
                                info.PhuongTien_Id = info_BienSo.Id;

                            }
                        }
                        if (string.IsNullOrEmpty(info.DonViSuDung))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Đơn vị không được để trống");
                        }
                        else
                        {
                            var info_DonVi = dataList.Where(x => !x.IsDeleted && x.TenDonVi.ToLower() == info.DonViSuDung.ToLower()).FirstOrDefault();
                            if (info_DonVi == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo đơn vị trong danh mục Đơn Vị");
                            }
                            else
                            {
                                info.DonVi_Id = info_DonVi.Id;

                            }
                        }
                        if (string.IsNullOrEmpty(info.BoPhanSuDung))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Bộ phận không được để trống");
                        }
                        else
                        {
                            var info_BoPhan = dataBoPhan.Where(x => !x.IsDeleted && x.TenPhongBan.ToLower().Contains(info.BoPhanSuDung.ToLower())).FirstOrDefault();
                            if (info_BoPhan == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo bộ phận trong danh mục Bộ phận");
                            }
                            else
                            {
                                info.BoPhan_Id = info_BoPhan.Id;

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
                            if (!string.IsNullOrEmpty(item.BienSo1))
                            {
                                if (DuplicateSoKhungs(list_datas.Where(x => x.BienSo1 == item.BienSo1).ToList()) > 0)
                                {
                                    item.IsLoi = true;
                                    item.lst_Lois.Add($"Biển số {item.BienSo1} bị trùng lặp");
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
        private int DuplicateSoKhungs(List<ImportMMS_DS_PhanBoDonVi> lst)
        {
            return lst.GroupBy(p => new { p.BienSo1 }).Where(p => p.Count() > 1).Count();
        }

        [HttpPost("LuuPhanBoDonVi")]
        public ActionResult Post_SaveImport(List<ImportMMS_DS_PhanBoDonVi> DH)
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
                    if (DateTime.TryParseExact(item.NgayBatDau, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime tempNgayDenKV))
                    {
                        ngayBatDau = tempNgayDenKV;
                    }
                    DateTime? ngayKetThuc = null;
                    if (DateTime.TryParseExact(item.NgayKetThuc, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime tempNgayKetThucKV))
                    {
                        ngayKetThuc = tempNgayKetThucKV;
                    }

                    var exit = uow.DS_PhuongTiens.GetSingle(x => !x.IsDeleted && x.BienSo1.ToLower() == item.BienSo1.ToLower());
                    if (exit == null)
                    {
                        return BadRequest($"Không tìm thấy dữ liệu biển số {item.BienSo1}");
                    }
                    else
                    {
                        // var exit = uow.KeHoachGiaoXes.GetSingle(x => x.SoKhung.ToLower() == item.SoKhung.ToLower());
                        exit.BienSo1 = item.BienSo1;
                        exit.DonVi_Id = item.DonVi_Id;
                        exit.BoPhan_Id = item.BoPhan_Id;
                        exit.UpdatedDate = DateTime.Now;
                        exit.UpdatedBy = Guid.Parse(User.Identity.Name);
                        uow.DS_PhuongTiens.Update(exit);

                        var LsPhanBo = new LichSuPhanBoDonVi();
                        LsPhanBo.Id = Guid.NewGuid();
                        LsPhanBo.PhuongTien_Id = exit.Id;
                        LsPhanBo.BienSo = item.BienSo1;
                        LsPhanBo.DonVi_Id = item.DonVi_Id;
                        LsPhanBo.BoPhan_Id = item.BoPhan_Id;
                        LsPhanBo.CreatedDate = DateTime.Now;
                        LsPhanBo.CreatedBy = Guid.Parse(User.Identity.Name);
                        LsPhanBo.NgayBatDau = ngayBatDau;
                        LsPhanBo.NgayKetThuc = ngayKetThuc?.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
                        uow.LichSuPhanBoDonVis.Add(LsPhanBo);

                    }

                }
                uow.Complete();
                return StatusCode(StatusCodes.Status200OK);
            }
        }




    }
}