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

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MMS_DS_ThietBiController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        public MMS_DS_ThietBiController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
        }

        [HttpGet]
        public ActionResult Get(string keyword, int page = 1)
        {
            if (keyword == null) keyword = "";
            var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).ToList();

            var data = uow.DS_Thietbis.GetAll(t => !t.IsDeleted
                ).Select(x => new
                {
                    x.Id,
                    x.TinhTrang,
                });
            if (page == -1)
            {
                return Ok(data);
            }
            else
            {
                int totalRow = data.Count();
                int pageSize = pageSizeData[0].PageSize;
                int totalPage = (int)Math.Ceiling(totalRow / (double)pageSize);

                // Kiểm tra và điều chỉnh giá trị của page
                if (page < 1)
                {
                    page = 1;
                }
                else if (page > totalPage)
                {
                    page = totalPage;
                }

                var datalist = data.Skip((page - 1) * pageSize).Take(pageSize);
                return Ok(new
                {
                    totalRow,
                    totalPage,
                    pageSize,
                    datalist
                });
            }
        }

        [HttpGet("ThietBi")]
        public ActionResult GetTaiXe(string keyword = null)
        {
            string[] include = { "PhuongTien" };
            var lichsu = uow.GhepNoiPhuongTien_ThietBis.GetAll(x => !x.IsDeleted, x => x.OrderByDescending(x => x.CreatedDate), include);
            string[] includes = { "DM_Loai", "DM_Model", "DM_TinhTrang" };
            var data = uow.DS_Thietbis.GetAll(t => !t.IsDeleted
            && (string.IsNullOrEmpty(keyword) || t.MaCode_BienSo1.ToLower().Contains(keyword.ToLower())
            || t.MaThietBi.Contains(keyword.ToUpper())), null, includes).Select(x => new
            {
                x.Id,
                x.MaThietBi,
                BienSo1 = x.MaCode_BienSo1,
                BienSo2 = x.MaCode_BienSo2,
                TenThietBi = x.Name,
                x.Name,
                Model = x.DM_Model?.Name,
                x.DM_Model?.Option,
                x.Model_Id,
                LoaiTB = x.DM_Loai?.Name,
                x.ThoiGianSuDung,
                x.ThoiGian_NgayBaoDuong,
                x.LoaiTB_Id,
                TinhTrang = x.DM_TinhTrang?.Name,

                PhanBo = lichsu.Where(t => t.ThietBi_Id == x.Id || t.ThietBi2_Id == x.Id)?.FirstOrDefault()?.PhuongTien?.BienSo1,
            });
            return Ok(data);
        }


        [HttpGet("GetById")]
        public ActionResult Get(Guid id)
        {
            string[] includes = { "DM_Loai", "DM_Model", "DM_TinhTrang" };
            var query = uow.DS_Thietbis.GetAll(x => x.Id == id, null, includes).Select(x => new
            {
                x.Id,
                x.MaThietBi,
                x.MaCode_BienSo1,
                x.MaCode_BienSo2,
                TenThietBi = x.Name,
                x.Name,
                Model = x.DM_Model?.Name,
                x.DM_Model?.Option,
                x.PhanBo,
                LoaiTB = x.DM_Loai?.Name,
                x.Model_Id,
                x.ThoiGianSuDung,
                x.LoaiTB_Id,
                x.TinhTrang_Id,
                TinhTrang = x.DM_TinhTrang?.Name,
            }).FirstOrDefault();
            if (query == null)
            {
                return NotFound();
            }
            return Ok(query);
        }
        [HttpGet("All")]
        public ActionResult GetAll(string keyword = null)
        {
            string[] includes = { "DM_Model", "DM_Loai", "DM_TinhTrang", "LichSuBaoDuong" };
            string[] include = { "DM_TanSuat" };
            var model = uow.DM_Models.GetAll(x => !x.IsDeleted, null, include);
            var data = uow.DS_Thietbis.GetAll(t => !t.IsDeleted
                && (string.IsNullOrEmpty(keyword) || t.Name.ToLower().Contains(keyword.ToLower())
                || t.MaThietBi.ToLower().Contains(keyword.ToLower())),
                t => t.OrderByDescending(x => x.DM_TinhTrang.Arrange), includes)
                .Select(x =>
                {
                    var dinhmuc = model.Where(t => t.Id == x.Model_Id)?.FirstOrDefault()?.DM_TanSuat?.GiaTri;
                    int giaTriDinhMuc = 0;
                    if (!string.IsNullOrEmpty(dinhmuc) && int.TryParse(dinhmuc, out int parsedValue))
                    {
                        giaTriDinhMuc = parsedValue;
                    }
                    return new
                    {
                        x.Id,
                        x.Name,
                        x.MaThietBi,
                        Model = x.DM_Model?.Name,
                        x.DM_Model?.Option,
                        x?.Model_Id,
                        x?.TinhTrang_Id,
                        TinhTrang = x.DM_TinhTrang?.Name,
                        x.Note,
                        x.ThoiGianSuDung,
                        LoaiPT = x.DM_Loai?.Name,
                        IsYeuCau = x.LichSuBaoDuong?.IsYeuCau ?? false,
                        IsDenHan = giaTriDinhMuc > 0 && x.ThoiGianSuDung >= giaTriDinhMuc,
                    };
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
                var loai = uow.DM_Loais.GetAll(x => !x.IsDeleted);
                var model = uow.DM_Models.GetAll(x => !x.IsDeleted);
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
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[5];
                    int rowCount = worksheet.Dimension.Rows;
                    var list_datas = new List<ImportMMS_DS_ThietBi>();
                    for (int i = 2; i <= rowCount; i++)
                    {
                        if (worksheet.Cells[i, 1].Value == null)
                        {
                            // Nếu không có dữ liệu, dừng vòng lặp
                            break;
                        }

                        object MaTB = worksheet.Cells[i, 1].Value;
                        object BienSo1 = worksheet.Cells[i, 2].Value;
                        object BienSo2 = worksheet.Cells[i, 3].Value;
                        object Name = worksheet.Cells[i, 4].Value;
                        object Model = worksheet.Cells[i, 5].Value;
                        object LoaiTB = worksheet.Cells[i, 6].Value;
                        object BoPhan = worksheet.Cells[i, 7].Value;
                        object ViTri = worksheet.Cells[i, 8].Value;
                        object Lat = worksheet.Cells[i, 9].Value;
                        object Long = worksheet.Cells[i, 10].Value;
                        object TinhTrang = worksheet.Cells[i, 11].Value;
                        object NgayBatDau = worksheet.Cells[i, 12].Value;



                        DateTime baseDate = new DateTime(1900, 1, 1);
                        int excelDate = 45329;
                        var lst_Lois = new List<string>();
                        var info = new ImportMMS_DS_ThietBi();
                        info.Id = Guid.NewGuid();
                        info.MaThietBi = MaTB?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.MaCode_BienSo1 = BienSo1?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.MaCode_BienSo2 = BienSo2?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.Name = Name?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.Model = Model?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.LoaiTB = LoaiTB?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.PhanBo = BoPhan?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.ViTri = ViTri?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.ViTri_Lat = Lat?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.ViTri_Long = Long?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.TinhTrang = TinhTrang?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";




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

                        if (string.IsNullOrEmpty(info.MaThietBi))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Mã phương tiện không được để trống");
                        }
                        if (string.IsNullOrEmpty(info.LoaiTB))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Loại TB không được để trống");
                        }
                        else
                        {
                            var info_loaiTB = loai.Where(x => x.Name.ToLower() == info.LoaiTB.ToLower()).FirstOrDefault();
                            if (info_loaiTB == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo loại TB trong danh mục Loại");
                            }
                            else
                            {
                                info.LoaiTB_Id = info_loaiTB.Id;

                            }

                        }

                        // if (string.IsNullOrEmpty(info.Model))
                        // {
                        //     info.IsLoi = true;
                        //     lst_Lois.Add("Model không được để trống");
                        // }
                        // else
                        // {
                        //     var info_model = model.Where(x => x.Name.ToLower() == info.Model.ToLower()).FirstOrDefault();
                        //     if (info_model == null)
                        //     {
                        //         info.IsLoi = true;
                        //         lst_Lois.Add("Chưa tạo model trong danh mục");
                        //     }
                        //     else
                        //     {
                        //         info.Model_Id = info_model.Id;

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
                            if (!string.IsNullOrEmpty(item.MaThietBi))
                            {
                                if (DuplicateSoKhungs(list_datas.Where(x => x.MaThietBi == item.MaThietBi).ToList()) > 0)
                                {
                                    item.IsLoi = true;
                                    item.lst_Lois.Add($"Số Khung {item.MaThietBi} bị trùng lặp");
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
        private int DuplicateSoKhungs(List<ImportMMS_DS_ThietBi> lst)
        {
            return lst.GroupBy(p => new { p.MaThietBi }).Where(p => p.Count() > 1).Count();
        }

        [HttpPost("Save_Import")]
        public ActionResult Post_SaveImport(List<ImportMMS_DS_ThietBi> DH)
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

                    var exit = uow.DS_Thietbis.GetSingle(x => !x.IsDeleted && x.MaThietBi.ToLower() == item.MaThietBi.ToLower());
                    if (exit == null)
                    {
                        uow.DS_Thietbis.Add(new DS_ThietBi
                        {
                            MaThietBi = item.MaThietBi,
                            MaCode_BienSo1 = item.MaCode_BienSo1,
                            MaCode_BienSo2 = item.MaCode_BienSo2,
                            Name = item.Name,
                            LoaiTB_Id = item.LoaiTB_Id,
                            TinhTrang = item.TinhTrang,
                            NgayBatDau = ngayBatDau,
                            LoaiTB = item.LoaiTB,
                            Model = item.Model,
                            PhanBo = item.PhanBo,
                            Model_Id = item.Model_Id,
                            ViTri = item.ViTri,
                            ViTri_Lat = item.ViTri_Lat,
                            ViTri_Long = item.ViTri_Long,
                            CreatedDate = DateTime.Now,
                            CreatedBy = Guid.Parse(User.Identity.Name),
                        });
                    }
                    else
                    {
                        // var exit = uow.KeHoachGiaoXes.GetSingle(x => x.SoKhung.ToLower() == item.SoKhung.ToLower());
                        exit.MaThietBi = item.MaThietBi;
                        exit.MaCode_BienSo1 = item.MaCode_BienSo1;
                        exit.MaCode_BienSo2 = item.MaCode_BienSo2;
                        exit.LoaiTB_Id = item.LoaiTB_Id;
                        exit.Model_Id = item.Model_Id;
                        exit.Name = item.Name;
                        exit.TinhTrang = item.TinhTrang;
                        exit.NgayBatDau = ngayBatDau;
                        exit.LoaiTB = item.LoaiTB;
                        exit.Model = item.Model;
                        exit.PhanBo = item.PhanBo;
                        exit.ViTri = item.ViTri;
                        exit.ViTri_Lat = item.ViTri_Lat;
                        exit.ViTri_Long = item.ViTri_Long;
                        exit.CreatedDate = DateTime.Now;
                        exit.CreatedBy = Guid.Parse(User.Identity.Name);
                        exit.UpdatedDate = DateTime.Now;
                        exit.UpdatedBy = Guid.Parse(User.Identity.Name);
                        uow.DS_Thietbis.Update(exit);
                    }

                }
                uow.Complete();
                return StatusCode(StatusCodes.Status200OK);
            }
        }

        [HttpGet("FileMau")]
        public ActionResult FileMauTM_DB()
        {
            string fullFilePath = Path.Combine(Directory.GetParent(environment.ContentRootPath).FullName, "Uploads/Templates/FileMau_DS_ThietBi.xlsx");
            string fileName = "FileMau_DS_ThietBi_" + DateTime.Now.ToString("HHmmss") + ".xlsx";
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

        [HttpPost]
        public ActionResult Post(DS_ThietBi data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (uow.DS_Thietbis.Exists(x => x.MaThietBi == data.MaThietBi && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.MaThietBi + " đã tồn tại trong hệ thống");
                else if (uow.DS_Thietbis.Exists(x => x.MaThietBi == data.MaThietBi && x.IsDeleted))
                {

                    var d = uow.DS_Thietbis.GetAll(x => x.MaThietBi == data.MaThietBi).FirstOrDefault();
                    d.IsDeleted = false;
                    d.DeletedBy = null;
                    d.DeletedDate = null;
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.Name = data.Name;
                    d.LoaiTB_Id = data.LoaiTB_Id;
                    d.Model_Id = data.Model_Id;
                    d.MaCode_BienSo1 = data.MaCode_BienSo1;
                    d.MaThietBi = data.MaThietBi;
                    d.MaCode_BienSo2 = data.MaCode_BienSo2;
                    d.Model = data.Model;
                    d.PhanBo = data.PhanBo;

                    uow.DS_Thietbis.Update(d);

                }
                else
                {
                    DS_ThietBi cv = new DS_ThietBi();
                    Guid id = Guid.NewGuid();
                    cv.Id = id;
                    cv.LoaiTB_Id = data.LoaiTB_Id;
                    cv.Model_Id = data.Model_Id;
                    cv.MaCode_BienSo1 = data.MaCode_BienSo1;
                    cv.MaThietBi = data.MaThietBi;
                    cv.ThoiGianSuDung = data.ThoiGianSuDung;
                    cv.MaCode_BienSo2 = data.MaCode_BienSo2;
                    cv.TinhTrang_Id = data.TinhTrang_Id;
                    cv.Name = data.Name;
                    cv.Model = data.Model;
                    cv.PhanBo = data.PhanBo;
                    cv.CreatedDate = DateTime.Now;
                    cv.CreatedBy = Guid.Parse(User.Identity.Name);
                    uow.DS_Thietbis.Add(cv);
                }

                uow.Complete();
                return Ok();
            }
        }


        [HttpPut]
        public ActionResult Put(Guid id, DS_ThietBi duLieu)
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
                uow.DS_Thietbis.Update(duLieu);
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
                DS_ThietBi duLieu = uow.DS_Thietbis.GetById(id);

                if (duLieu == null)
                {
                    return NotFound();
                }
                duLieu.DeletedDate = DateTime.Now;
                duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                duLieu.IsDeleted = true;
                uow.DS_Thietbis.Update(duLieu);
                uow.Complete();
                return Ok(duLieu);
            }

        }

    }
}
