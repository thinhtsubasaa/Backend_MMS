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
using System.Security.Claims;
using ERP.Helpers;
using System.Threading.Tasks;
using System.Net.Http;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MMS_KeHoachController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        private readonly DataService _master;
        public MMS_KeHoachController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment, DataService master)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
            _master = master;
        }

        [HttpGet("BaoDuong")]
        public async Task<ActionResult> Get(DateTime TuNgay, DateTime DenNgay, string keyword = null)
        {
            var dataList = await _master.GetDiadiem();
            Expression<Func<LichSuBaoDuong, bool>> whereFunc = item => !item.IsDeleted
            && item.PhuongTien_Id != null
            && item.Ngay.Date >= TuNgay && item.Ngay.Date <= DenNgay
            && !item.IsBaoDuong
            && (string.IsNullOrEmpty(keyword) ||
            item.PhuongTien.BienSo1.Contains(keyword.Trim().ToLower())
            || item.PhuongTien.BienSo2.Contains(keyword.Trim().ToLower()));
            string[] includes = { "BaoDuong", "PhuongTien",
            "LichSuBaoDuong_ChiTiets.HangMuc",
            "PhuongTien.DM_Model","PhuongTien.DM_Loai","PhuongTien.PhuTrachBoPhan"
           };
            // HttpClient httpClient = new HttpClient();
            // VehicleService vehicleService = new VehicleService(httpClient);
            var data = uow.LichSuBaoDuongs.GetAll(whereFunc, x => x.OrderByDescending(x => x.CreatedDate), includes)
             .Select(x =>
             {
                 var now = DateTime.Now;
                 var ngaySuDung = x.NgayDeXuatHoanThanh != null ? (now - x.NgayDeXuatHoanThanh)?.Days : null;
                 //  string plate = x.PhuongTien?.BienSo1?.Replace("-", "");
                 //  VehicleInfo vehicle = await vehicleService.GetVehicleByPlateAsync(plate);
                 //  string toaDo = vehicle != null ? $"{vehicle.Lat},{vehicle.Lng}" : $"{0,0}";
                 return new
                 {
                     x.Id,
                     x.DiaDiem_Id,
                     LoaiBaoDuong = x.BaoDuong?.Name,
                     x.PhuongTien?.MaPhuongTien,
                     LoaiPT = x.PhuongTien?.DM_Loai?.Name,
                     Model = x.PhuongTien?.DM_Model?.Name,
                     x.PhuongTien?.DM_Model?.Option,
                     NguoiPhuTrach = x.PhuongTien?.PhuTrachBoPhan?.NhanVien,
                     SoNgaySuDung = ngaySuDung,
                     TenDiaDiem = dataList.Where(t => !x.IsDeleted && t.Id == x.DiaDiem_Id)?.FirstOrDefault()?.TenDiaDiem,
                     NgayDiBaoDuong = string.Format("{0:dd/MM/yyyy }", x.NgayDiBaoDuong),
                     NgayHoanThanh = string.Format("{0:dd/MM/yyyy }", x.NgayDeXuatHoanThanh),
                     x.BaoDuong_Id,
                     x.NguoiYeuCau,
                     x.BaoDuong?.TanSuat,
                     GiaTri = x.BaoDuong?.GiaTri.ToString(),
                     x.PhuongTien_Id,
                     x.PhuongTien?.BienSo1,
                     SoKM = x.PhuongTien?.SoKM.ToString(),
                     SoChuyenXe = x.PhuongTien?.SoChuyenXe.ToString(),
                     ChiSo = x.BaoDuong?.GiaTri.ToString(),
                     x.NoiDung,
                     //  ToaDo = toaDo,
                     x.IsDuyet,
                     x.IsYeuCau,
                     x.IsBaoDuong,
                     x.HinhAnh,
                     x.KetQua,
                     ChiPhi = x.ChiPhi.ToString(),
                     lichSu = x.LichSuBaoDuong_ChiTiets.Where(a => !a.IsDeleted && a.LichSuBaoDuong_Id == x.Id
                 ).Select(x => new
                 {
                     x.Id,
                     x.LichSuBaoDuong_Id,
                     x.HangMuc?.NoiDungBaoDuong,
                     DinhMuc = string.Format("{0:N0}", x.HangMuc?.DinhMuc),
                     //  x.HangMuc?.DinhMuc,
                     x.HangMuc?.LoaiBaoDuong,
                     x.HangMuc?.GhiChu,
                     x.LichSuBaoDuong?.PhuongTien?.BienSo1,
                     x.LichSuBaoDuong?.PhuongTien?.BienSo2,

                 }),
                 };
             });
            return Ok(data);
        }
        [HttpGet("Map")]
        public async Task<ActionResult> GetMap(string keyword)
        {
            HttpClient httpClient = new HttpClient();
            VehicleService vehicleService = new VehicleService(httpClient);
            string plate = keyword.Replace("-", "");
            VehicleInfo vehicle = await vehicleService.GetVehicleByPlateAsync(plate);
            string toaDo = vehicle != null ? $"{vehicle.Lat},{vehicle.Lng}" : $"{0,0}";
            return Ok(toaDo);
        }

        [HttpGet("GetById")]
        public async Task<ActionResult> GetEdit(Guid? Id)
        {
            var dataList = await _master.GetDiadiem();
            Expression<Func<LichSuBaoDuong, bool>> whereFunc = item => !item.IsDeleted && item.Id == Id;
            string[] includes = { "BaoDuong", "PhuongTien", "LichSuBaoDuong_ChiTiets.HangMuc",
            "LichSuBaoDuong_ChiTiets.HangMuc.DM_TanSuat","LichSuBaoDuong_ChiTiets.PhuongTien" };
            var data = uow.LichSuBaoDuongs.GetAll(whereFunc, null, includes)
             .Select(x => new
             {
                 x.Id,
                 x.BaoDuong_Id,
                 TenDiaDiem = dataList.Where(t => !x.IsDeleted && t.Id == x.DiaDiem_Id)?.FirstOrDefault()?.TenDiaDiem,
                 NgayDiBaoDuong = string.Format("{0:dd/MM/yyyy}", x.NgayDiBaoDuong),
                 x.Ngay,
                 x.NgayXacNhan,
                 x.NgayHoanThanh,
                 x.DiaDiem_Id,
                 LoaiBaoDuong = x.BaoDuong?.Name,
                 x.BaoDuong?.TanSuat,
                 x.BaoDuong?.GiaTri,
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
                    x.HangMuc_Id,
                    TieuChi = x.HangMuc.DM_TanSuat?.TanSuat,
                    x.PhuongTien?.SoKM,
                    x.PhuongTien?.SoKM_NgayBaoDuong,
                    x.LichSuBaoDuong?.PhuongTien?.BienSo1,
                    x.LichSuBaoDuong?.PhuongTien?.BienSo2,
                }),
             }).FirstOrDefault();
            return Ok(data);
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
                    var list_datas = new List<ImportMMS_KeHoach>();
                    for (int i = 2; i <= rowCount; i++)
                    {
                        if (worksheet.Cells[i, 3].Value == null)
                        {
                            // Nếu không có dữ liệu, dừng vòng lặp
                            break;
                        }
                        // object NgayBatDau = worksheet.Cells[i, 1].Value;
                        object BienSo1 = worksheet.Cells[i, 1].Value;
                        object BienSo2 = worksheet.Cells[i, 2].Value;
                        object LoaiBaoDuong = worksheet.Cells[i, 3].Value;
                        object TanSuat = worksheet.Cells[i, 4].Value;
                        object GiaTri = worksheet.Cells[i, 5].Value;
                        object DiaDiem = worksheet.Cells[i, 6].Value;

                        DateTime baseDate = new DateTime(1900, 1, 1);
                        int excelDate = 45329;
                        var lst_Lois = new List<string>();
                        var info = new ImportMMS_KeHoach();
                        info.Id = Guid.NewGuid();
                        info.BienSo1 = BienSo1?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.BienSo2 = BienSo2?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.LoaiBaoDuong = LoaiBaoDuong?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.TanSuat = TanSuat?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.GiaTri = GiaTri?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.MaDiaDiem = DiaDiem?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";

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

                        // var ngayBatDau = NgayBatDau?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        // info.NgayBatDau = ProcessDateString(ngayBatDau, baseDate);

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
        private int DuplicateSoKhungs(List<ImportMMS_KeHoach> lst)
        {
            return lst.GroupBy(p => new { p.LoaiBaoDuong }).Where(p => p.Count() > 1).Count();
        }

        [HttpPost("Save_Import")]
        public ActionResult Post_SaveImport(List<ImportMMS_KeHoach> DH)
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


                    // DateTime? ngayBatDau = null;
                    // if (DateTime.TryParseExact(item.NgayBatDau, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime tempNgayDenKV))
                    // {
                    //     ngayBatDau = tempNgayDenKV;
                    // }

                    var exit = uow.LichSuBaoDuongs.GetSingle(x => !x.IsDeleted && x.BaoDuong.Name == item.LoaiBaoDuong.ToUpper());
                    if (exit == null)
                    {
                        // uow.LichSuBaoDuongs.Add(new LichSuBaoDuong
                        // {
                        //     Id = Guid.NewGuid(),
                        //     PhuongTien_Id = item.PhuongTien_Id,
                        //     BaoDuong_Id = item.BaoDuong_Id,
                        //     DiaDiem_Id = item.DiaDiem_Id,
                        //     Ngay = DateTime.Now,
                        //     NgayXacNhan = DateTime.Now,
                        //     IsDuyet = true,
                        //     IsYeuCau = true,
                        //     NguoiYeuCau = item.NguoiYeuCau,
                        //     NguoiXacNhan = item.NguoiXacNhan,
                        //     NguoiXacNhan_Id = Guid.Parse(User.Identity.Name),
                        //     NguoiYeuCau_Id = Guid.Parse(User.Identity.Name),
                        //     CreatedDate = DateTime.Now,
                        //     CreatedBy = Guid.Parse(User.Identity.Name),
                        // });
                        var newLichSuBaoDuong = new LichSuBaoDuong
                        {
                            Id = Guid.NewGuid(),
                            PhuongTien_Id = item.PhuongTien_Id,
                            BaoDuong_Id = item.BaoDuong_Id,
                            DiaDiem_Id = item.DiaDiem_Id,
                            Ngay = DateTime.Now,
                            NgayXacNhan = DateTime.Now,
                            IsDuyet = true,
                            IsYeuCau = true,
                            NguoiYeuCau = item.NguoiYeuCau,
                            NguoiXacNhan = item.NguoiXacNhan,
                            NguoiXacNhan_Id = Guid.Parse(User.Identity.Name),
                            NguoiYeuCau_Id = Guid.Parse(User.Identity.Name),
                            CreatedDate = DateTime.Now,
                            CreatedBy = Guid.Parse(User.Identity.Name),
                        };
                        // Thêm bản ghi mới
                        uow.LichSuBaoDuongs.Add(newLichSuBaoDuong);
                        var phuongtien = uow.DS_PhuongTiens.GetById(item.PhuongTien_Id);
                        if (phuongtien == null)
                        {
                            return BadRequest("Không tìm thấy phương tiện ");
                        }
                        phuongtien.TinhTrang_Id = Guid.Parse("3fcfbc57-b53e-4226-8251-674c5a839b60");
                        phuongtien.LichSuBaoDuong_Id = newLichSuBaoDuong.Id;
                        uow.DS_PhuongTiens.Update(phuongtien);
                    }
                    else
                    {
                        // var exit = uow.KeHoachGiaoXes.GetSingle(x => x.SoKhung.ToLower() == item.SoKhung.ToLower());
                        exit.PhuongTien_Id = item.PhuongTien_Id;
                        exit.BaoDuong_Id = item.BaoDuong_Id;
                        exit.DiaDiem_Id = item.DiaDiem_Id;
                        exit.Ngay = DateTime.Now;
                        exit.NgayXacNhan = DateTime.Now;
                        exit.IsDuyet = true;
                        exit.IsYeuCau = true;
                        exit.NguoiYeuCau = item.NguoiYeuCau;
                        exit.NguoiXacNhan = item.NguoiXacNhan;
                        exit.NguoiXacNhan_Id = Guid.Parse(User.Identity.Name);
                        exit.NguoiYeuCau_Id = Guid.Parse(User.Identity.Name);
                        exit.UpdatedDate = DateTime.Now;
                        exit.UpdatedBy = Guid.Parse(User.Identity.Name);
                        uow.LichSuBaoDuongs.Update(exit);
                    }
                }
                uow.Complete();
                return StatusCode(StatusCodes.Status200OK);
            }
        }

        [HttpPost]
        public ActionResult Post(LichSuBaoDuong data, [FromQuery] List<string> ids)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // if (uow.KeHoachBaoDuongs.Exists(x => !x.IsDeleted && x.PhuongTien.BienSo1 == data.PhuongTien.BienSo1 ))
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
                dv.Ngay = DateTime.Now;
                dv.NgayXacNhan = DateTime.Now;
                dv.CreatedDate = DateTime.Now;
                dv.IsYeuCau = true;
                dv.IsDuyet = true;
                dv.NguoiYeuCau = data.NguoiYeuCau;
                dv.NguoiXacNhan = data.NguoiXacNhan;
                dv.NguoiXacNhanHoanThanh_Id = Guid.Parse(User.Identity.Name);
                dv.NgayDiBaoDuong = data.NgayDiBaoDuong;
                dv.NguoiYeuCau_Id = Guid.Parse(User.Identity.Name);
                dv.CreatedBy = Guid.Parse(User.Identity.Name);
                dv.BaoDuong_Id = data.BaoDuong_Id;
                dv.PhuongTien_Id = data.PhuongTien_Id;

                uow.LichSuBaoDuongs.Add(dv);
                var phuongtien = uow.DS_PhuongTiens.GetById(data.PhuongTien_Id);
                if (phuongtien == null)
                {
                    return BadRequest("Không tìm thấy phương tiện ");
                }
                phuongtien.LichSuBaoDuong_Id = dv.Id;
                phuongtien.TinhTrang_Id = Guid.Parse("3fcfbc57-b53e-4226-8251-674c5a839b60");
                uow.DS_PhuongTiens.Update(phuongtien);
                var lichSuBaoDuongList = new List<LichSuBaoDuong_ChiTiet>();
                foreach (var idct in ids)
                {
                    var chitiet = new LichSuBaoDuong_ChiTiet();
                    chitiet.Id = Guid.NewGuid();
                    chitiet.LichSuBaoDuong_Id = dv.Id;
                    chitiet.PhuongTien_Id = data.PhuongTien_Id;
                    chitiet.HangMuc_Id = Guid.Parse(idct);
                    chitiet.CreatedDate = DateTime.Now;
                    chitiet.CreatedBy = Guid.Parse(User.Identity.Name);
                    lichSuBaoDuongList.Add(chitiet);
                }
                uow.LichSuBaoDuong_ChiTiets.AddRange(lichSuBaoDuongList);
                // }

                uow.Complete();
                return Ok();
            }
        }


        [HttpGet("FileMau")]
        public ActionResult FileMauTM_DB()
        {
            string fullFilePath = Path.Combine(Directory.GetParent(environment.ContentRootPath).FullName, "Uploads/Templates/FileMau_KeHoachBaoDuong.xlsx");
            string fileName = "FileMau_KeHoachBaoDuong_" + DateTime.Now.ToString("HHmmss") + ".xlsx";
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

        [HttpPut]
        public ActionResult Put(Guid id, KeHoachBaoDuong duLieu)
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
                uow.KeHoachBaoDuongs.Update(duLieu);
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
                KeHoachBaoDuong duLieu = uow.KeHoachBaoDuongs.GetById(id);

                if (duLieu == null)
                {
                    return NotFound();
                }
                duLieu.DeletedDate = DateTime.Now;
                duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                duLieu.IsDeleted = true;
                uow.KeHoachBaoDuongs.Update(duLieu);
                uow.Complete();
                return Ok(duLieu);
            }

        }
    }
}
