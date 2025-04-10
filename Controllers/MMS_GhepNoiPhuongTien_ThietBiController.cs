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
using System.Threading;
namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MMS_GhepNoiPhuongTien_ThietBiController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        private readonly DbAdapter dbAdapter;
        private readonly DataService _master;
        private readonly IConfiguration configuration;
        public MMS_GhepNoiPhuongTien_ThietBiController(IConfiguration _configuration, IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment, DataService master)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
            _master = master;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            dbAdapter = new DbAdapter(connectionString);
        }

        [HttpGet("PhuongTien")]
        public async Task<ActionResult> GetPT(Guid? DonVi_Id, string keyword = null)
        {
            var dataDonVi = await _master.GetDonVi();
            var dataBoPhan = await _master.GetBoPhan();

            var phanbo = uow.GhepNoiPhuongTien_ThietBis.GetAll(x => !x.IsDeleted, x => x.OrderByDescending(x => x.CreatedDate)).ToList();

            var data = uow.DS_PhuongTiens.GetAll(t => !t.IsDeleted
            && (DonVi_Id == null || t.DonVi_Id == DonVi_Id)
            && (string.IsNullOrEmpty(keyword) || t.BienSo1.ToLower().Contains(keyword.ToLower())
            || t.BienSo2.ToLower().Contains(keyword.ToLower())), t => t.OrderByDescending(x => x.NgayBatDau)
                ).Select(x =>
                {
                    var ngayKetThuc = phanbo.Where(t => t.PhuongTien_Id == x.Id)?.FirstOrDefault()?.NgayKetThuc;
                    var UpdateBienSo = (ngayKetThuc.HasValue && ngayKetThuc.Value < DateTime.Now) ? null : phanbo.Where(t => t.PhuongTien_Id == x.Id)?.FirstOrDefault()?.TenThietBi;
                    return new
                    {
                        x.Id,
                        x.BienSo1,
                        x.SoKhung,
                        x.DonVi_Id,
                        x.BienSo2,
                        TenThietBi = UpdateBienSo,
                        x.BoPhan_Id,
                        DonViSuDung = dataDonVi.Where(t => !t.IsDeleted && t.Id == x.DonVi_Id)?.FirstOrDefault()?.TenDonVi,
                        TenBoPhan = dataBoPhan.Where(t => !t.IsDeleted && t.Id == x.BoPhan_Id)?.FirstOrDefault()?.TenPhongBan,
                        x.MaPhuongTien,
                        NgayBatDau = string.Format("{0:dd/MM/yyyy}", phanbo.Where(t => t.PhuongTien_Id == x.Id)?.FirstOrDefault()?.NgayBatDau),
                        NgayKetThuc = string.Format("{0:dd/MM/yyyy}", phanbo.Where(t => t.PhuongTien_Id == x.Id)?.FirstOrDefault()?.NgayKetThuc),
                    };
                });

            return Ok(data);
        }


        [HttpGet("PhuTrach")]
        public async Task<ActionResult> GetPhuTrach(Guid? BoPhan_Id, string keyword)
        {
            var dataDonVi = await _master.GetDonVi();
            var dataUser = await _master.GetUser();
            var dataUserNet = await _master.GetUserNet();

            var query = dataUser.Where(x => !x.IsDeleted && x.User_Id != null && (BoPhan_Id == null || x.PhongBan_Id == BoPhan_Id)
            && dataUserNet.Any(t => !t.IsDeleted && t.Id == x.User_Id)
            ).ToList()
            .Select(x =>
            {
                var userNet = dataUserNet.FirstOrDefault(t => !t.IsDeleted && t.Id == x.User_Id);
                var tenNhanVien = userNet != null ? $"{userNet.FullName}-{userNet.MaNhanVien}" : null;
                return new
                {
                    key = x.Id,
                    x.Id,
                    x.DonVi_Id,
                    x.PhongBan_Id,
                    x.User_Id,
                    TenDonVi = dataDonVi.Where(t => !t.IsDeleted && t.Id == x.DonVi_Id)?.FirstOrDefault()?.TenDonVi,
                    TenNhanVien = userNet != null ? $"{userNet.FullName}-{userNet.MaNhanVien}" : null
                };
            });
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.TenNhanVien != null && x.TenNhanVien.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            return Ok(query);
        }

        [HttpGet("ChiTiet")]
        public async Task<ActionResult> GetChiTiet(Guid? PhuongTien_Id)
        {
            var dataBoPhan = await _master.GetBoPhan();
            var dataDonVi = await _master.GetDonVi();

            string[] includes = { "PhuongTien" };
            var query = uow.GhepNoiPhuongTien_ThietBis.GetAll(x => !x.IsDeleted && (PhuongTien_Id == null || x.PhuongTien_Id == PhuongTien_Id), x => x.OrderByDescending(x => x.CreatedDate), includes)
            .Select(x => new
            {
                key = x.Id,
                x.Id,
                x.PhuongTien?.BienSo1,
                NgayBatDau = string.Format("{0:dd/MM/yyyy}", x.NgayBatDau),
                NgayKetThuc = string.Format("{0:dd/MM/yyyy}", x.NgayKetThuc),
                TenDonVi = dataDonVi.Where(t => !t.IsDeleted && t.Id == x.DonVi_Id)?.FirstOrDefault()?.TenDonVi,
                x.TenThietBi,
                TenBoPhan = dataBoPhan.Where(t => !t.IsDeleted && t.Id == x.BoPhan_Id)?.FirstOrDefault()?.TenPhongBan,
                x.DonVi_Id,
                x.BoPhan_Id,
                x.ThietBi_Id,
                x.ThietBi2_Id,
            });

            return Ok(query);
        }
        [HttpPost("Read_Excel")]
        public async Task<ActionResult> Read_ExcelAsync(IFormFile file)
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            DateTime dt = DateTime.Now;
            // Rename file
            string fileName = (long)timeSpan.TotalSeconds + "_" + Commons.TiengVietKhongDau(file.FileName);
            string fileExt = fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower();
            string[] supportedTypes = new[] { "xls", "xlsx" };
            if (supportedTypes.Contains(fileExt))
            {
                var dataList = uow.DS_PhuongTiens.GetAll(x => !x.IsDeleted).Select(t => new { t.Id, t.BienSo1 }).ToList();
                var dataDonVi = await _master.GetDonVi();
                var thietbi = uow.DS_Thietbis.GetAll(x => !x.IsDeleted).Select(t => new { t.Id, t.MaThietBi, t.Name }).ToList();
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
                    var list_datas = new List<ImportMMS_GhepNoiPT_TB>();
                    for (int i = 1; i <= rowCount; i++)
                    {
                        if (worksheet.Cells[i, 3].Value == null)
                        {
                            // Nếu không có dữ liệu, dừng vòng lặp
                            break;
                        }
                        object MaThietBi = worksheet.Cells[i, 1].Value;
                        object MaThietBi2 = worksheet.Cells[i, 2].Value;
                        object BienSo1 = worksheet.Cells[i, 3].Value;
                        object DonViSuDung = worksheet.Cells[i, 4].Value;

                        object NgayBatDau = worksheet.Cells[i, 5].Value;
                        object NgayKetThuc = worksheet.Cells[i, 6].Value;

                        DateTime baseDate = new DateTime(1900, 1, 1);
                        int excelDate = 45329;
                        var lst_Lois = new List<string>();
                        var info = new ImportMMS_GhepNoiPT_TB();
                        info.Id = Guid.NewGuid();
                        info.MaThietBi = MaThietBi?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.MaThietBi2 = MaThietBi2?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.BienSo1 = BienSo1?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.DonViSuDung = DonViSuDung?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";


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

                        if (string.IsNullOrEmpty(info.MaThietBi))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Mã thiết bị không được để trống");
                        }
                        if (string.IsNullOrEmpty(info.DonViSuDung))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Đơn vị không được để trống");
                        }
                        else if (info.MaThietBi != null)
                        {
                            var info_ThietBi = thietbi.Where(x => x.MaThietBi == info.MaThietBi).FirstOrDefault();
                            if (info_ThietBi == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo thiết bị trong danh sách thiết bị");
                            }
                            else
                            {
                                info.ThietBi_Id = info_ThietBi.Id;

                            }
                        }
                        else if (info.MaThietBi2 != null)
                        {
                            var info_ThietBi2 = thietbi.Where(x => x.MaThietBi == info.MaThietBi2).FirstOrDefault();
                            if (info_ThietBi2 == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo thiết bị trong danh sách thiết bị");
                            }
                            else
                            {
                                info.ThietBi2_Id = info_ThietBi2.Id;

                            }

                        }
                        if (string.IsNullOrEmpty(info.BienSo1))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Biển số không được để trống");
                        }
                        else
                        {
                            var info_BienSo = dataList.Where(x => x.BienSo1.ToLower().Replace("-", "").Replace(".", "").Contains(info.BienSo1.ToLower().Replace("-", "").Replace(".", ""))).FirstOrDefault();
                            if (info_BienSo == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo biển số trong danh sách biển số");
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
                            var info_DonVi = dataDonVi.Where(x => !x.IsDeleted && x.TenDonVi.ToLower().Contains(info.DonViSuDung.ToLower())).FirstOrDefault();
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
        private int DuplicateSoKhungs(List<ImportMMS_GhepNoiPT_TB> lst)
        {
            return lst.GroupBy(p => new { p.BienSo1 }).Where(p => p.Count() > 1).Count();
        }

        [HttpPost("LuuGhepNoiPhuongTien")]
        public ActionResult Post_SaveImportAsync(List<ImportMMS_GhepNoiPT_TB> DH)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                IFormatProvider fm = new CultureInfo("en-US", true);
                var dataThietBi = uow.DS_Thietbis.GetAll(x => !x.IsDeleted).ToList();

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
                    if (item.ThietBi_Id == null)
                    {
                        return BadRequest($"Không tìm thấy dữ liệu thiết bị");
                    }
                    var exit = uow.DS_PhuongTiens.GetSingle(x => !x.IsDeleted && x.Id == item.PhuongTien_Id);
                    if (exit == null)
                    {
                        return BadRequest($"Không tìm thấy dữ liệu biển số {item.BienSo1}");
                    }
                    else
                    {
                        var thietbi1 = item.ThietBi_Id != null
    ? dataThietBi.Where(t => !t.IsDeleted && t.Id == item.ThietBi_Id)
                 .Select(t => t.Name)
                 .FirstOrDefault()
    : null;

                        var thietbi2 = item.ThietBi2_Id != null
    ? dataThietBi.Where(t => !t.IsDeleted && t.Id == item.ThietBi2_Id)
                 .Select(t => t.Name)
                 .FirstOrDefault()
    : null;

                        var LsPhanBo = new GhepNoiPhuongTien_ThietBi();
                        LsPhanBo.Id = Guid.NewGuid();
                        LsPhanBo.PhuongTien_Id = item.PhuongTien_Id;
                        LsPhanBo.DonVi_Id = item.DonVi_Id;
                        LsPhanBo.BoPhan_Id = item.BoPhan_Id;
                        LsPhanBo.ThietBi_Id = item.ThietBi_Id;
                        LsPhanBo.ThietBi2_Id = item.ThietBi2_Id;
                        //     LsPhanBo.NhanVien = dataUserNet
                        // .Where(t => !t.IsDeleted && t.Id == item.User_Id)
                        // .Select(t => t.FullName + "-" + t.MaNhanVien)
                        // .FirstOrDefault();
                        LsPhanBo.TenThietBi = string.Join(", ", new[] { thietbi1, thietbi2 }.Where(u => !string.IsNullOrEmpty(u)));

                        LsPhanBo.CreatedDate = DateTime.Now;
                        LsPhanBo.CreatedBy = Guid.Parse(User.Identity.Name);
                        LsPhanBo.NgayBatDau = ngayBatDau;
                        LsPhanBo.NgayKetThuc = ngayKetThuc?.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
                        uow.GhepNoiPhuongTien_ThietBis.Add(LsPhanBo);

                    }
                }
                uow.Complete();
                return StatusCode(StatusCodes.Status200OK);
            }
        }

    }
}