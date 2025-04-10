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
    public class MMS_DM_HangMucController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        public MMS_DM_HangMucController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
        }

        [HttpPost("AddThongTin")]
        public ActionResult SyncPhuongTienHangMuc()
        {
            lock (Commons.LockObjectState)
            {
                // 🔥 Lấy tất cả phương tiện (chưa bị xóa)
                var danhSachPhuongTien = uow.DS_PhuongTiens.GetAll(x => !x.IsDeleted).Select(x => x.Id).ToList();

                // 🔥 Lấy tất cả hạng mục
                var danhSachHangMuc = uow.DM_HangMucs.GetAll(x => !x.IsDeleted).Select(x => x.Id).ToList();

                // 🔥 Kiểm tra dữ liệu
                if (!danhSachPhuongTien.Any() || !danhSachHangMuc.Any())
                {
                    return BadRequest("Không có phương tiện hoặc hạng mục nào để đồng bộ.");
                }

                // 🔥 Duyệt từng phương tiện và gán với tất cả hạng mục
                foreach (var phuongTienId in danhSachPhuongTien)
                {
                    foreach (var hangMucId in danhSachHangMuc)
                    {
                        // 🔍 Kiểm tra xem đã có dữ liệu này chưa để tránh trùng lặp
                        bool exists = uow.ThongTinTheoHangMucs.Exists(x => x.PhuongTien_Id == phuongTienId && x.HangMuc_Id == hangMucId);
                        if (!exists)
                        {
                            uow.ThongTinTheoHangMucs.Add(new ThongTinTheoHangMuc
                            {
                                PhuongTien_Id = phuongTienId,
                                HangMuc_Id = hangMucId,
                                CreatedDate = DateTime.Now,
                                CreatedBy = Guid.Parse(User.Identity.Name),
                            });
                        }
                    }
                }
                uow.Complete();
                return Ok();
            }
        }


        [HttpGet("HangMuc")]
        public ActionResult Get(string keyword = null)
        {
            string[] includes = { "DM_TanSuat", "DM_Loai" };
            var data = uow.DM_HangMucs.GetAll(t => !t.IsDeleted
            && (string.IsNullOrEmpty(keyword)
            || t.NoiDungBaoDuong.ToLower().Contains(keyword.ToLower())),
            x => x.OrderByDescending(x => x.DinhMuc > 0).ThenBy(x => x.CreatedDate), includes
                ).Select(x => new
                {
                    x.Id,
                    x.DM_TanSuat?.TanSuat,
                    x.NoiDungBaoDuong,
                    x.LoaiBaoDuong,
                    // x.DinhMuc,
                    DinhMuc = string.Format("{0:N0}", x.DinhMuc),
                    LoaiPT = x?.DM_Loai?.Name,
                    // x.CanhBao_DenHan,
                    CanhBao_DenHan = string.Format("{0:N0}", x.CanhBao_DenHan),
                    // x.CanhBao_GanDenHan,
                    CanhBao_GanDenHan = string.Format("{0:N0}", x.CanhBao_GanDenHan),
                    DinhMuc2 = x.DinhMuc.ToString(),
                    x.GhiChu,
                });
            return Ok(data);
        }

        [HttpGet("GetById")]
        public ActionResult Get(Guid id)
        {
            string[] includes = { "DM_TanSuat", "DM_Loai" };
            var query = uow.DM_HangMucs.GetAll(x => x.Id == id, null, includes).Select(x => new
            {
                x.Id,
                x.DM_TanSuat?.TanSuat,
                x.NoiDungBaoDuong,
                x.DinhMuc,
                LoaiPT = x?.DM_Loai?.Name,
                x.LoaiPT_Id,
                x.CanhBao_DenHan,
                x.CanhBao_GanDenHan,
                x.LoaiBaoDuong,
                x.GhiChu,
                x.TanSuat_Id,
            }).FirstOrDefault();
            if (query == null)
            {
                return NotFound();
            }
            return Ok(query);
        }
        [HttpGet("KiemTraThuongXuyen")]
        public ActionResult GetKiemTra(string keyword = null)
        {
            var data = uow.DM_HangMucs.GetAll(t => !t.IsDeleted
            && t.GhiChu == "Kiểm tra thường xuyên").Select(x => new
            {
                x.Id,
                x.NoiDungBaoDuong,
                x.LoaiBaoDuong,
                DinhMuc = string.Format("{0:N0}", x.DinhMuc),
                DinhMuc2 = string.Format("{0:N0}", x.DinhMuc),
                x.GhiChu,
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
                var tanSuat = uow.DM_TanSuats.GetAll(x => !x.IsDeleted);
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
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];
                    int rowCount = worksheet.Dimension.Rows;
                    var list_datas = new List<ImportMMS_DM_HangMuc>();
                    for (int i = 2; i <= rowCount; i++)
                    {
                        if (worksheet.Cells[i, 2].Value == null)
                        {
                            // Nếu không có dữ liệu, dừng vòng lặp
                            break;
                        }

                        object NoiDungBaoDuong = worksheet.Cells[i, 2].Value;
                        object DinhMuc = worksheet.Cells[i, 3].Value;
                        object LoaiBaoDuong = worksheet.Cells[i, 4].Value;
                        object GhiChu = worksheet.Cells[i, 5].Value;
                        object TenTanSuat = worksheet.Cells[i, 6].Value;

                        DateTime baseDate = new DateTime(1900, 1, 1);
                        int excelDate = 45329;
                        var lst_Lois = new List<string>();
                        var info = new ImportMMS_DM_HangMuc();
                        info.Id = Guid.NewGuid();
                        info.NoiDungBaoDuong = NoiDungBaoDuong?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.DinhMuc = DinhMuc?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.LoaiBaoDuong = LoaiBaoDuong?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.GhiChu = GhiChu?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.TenTanSuat = TenTanSuat?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";

                        if (string.IsNullOrEmpty(info.NoiDungBaoDuong))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Nội dung không được để trống");
                        }
                        if (string.IsNullOrEmpty(info.TenTanSuat))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Loại tiêu chí không được để trống");
                        }
                        else
                        {
                            var info_tanSuat = tanSuat.Where(x => x.TanSuat.ToLower() == info.TenTanSuat.ToLower()).FirstOrDefault();
                            if (info_tanSuat == null)
                            {
                                info.IsLoi = true;
                                lst_Lois.Add("Chưa tạo tiêu chí trong danh mục");
                            }
                            else
                            {
                                info.TanSuat_Id = info_tanSuat.Id;
                                if (info.TanSuat_Id == Guid.Parse("973a6901-f995-491e-83c5-5ca67d02c294"))
                                {
                                    info.TanSuat_Id = Guid.Parse("bccd28fb-2e36-4bae-9dbd-ce389d409a95");
                                    info.DinhMuc = (int.Parse(info.DinhMuc) * 30).ToString();
                                }

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
                            if (!string.IsNullOrEmpty(item.NoiDungBaoDuong))
                            {
                                if (DuplicateSoKhungs(list_datas.Where(x => x.NoiDungBaoDuong == item.NoiDungBaoDuong).ToList()) > 0)
                                {
                                    item.IsLoi = true;
                                    item.lst_Lois.Add($"Nội dung {item.NoiDungBaoDuong} bị trùng lặp");
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
        private int DuplicateSoKhungs(List<ImportMMS_DM_HangMuc> lst)
        {
            return lst.GroupBy(p => new { p.NoiDungBaoDuong }).Where(p => p.Count() > 1).Count();
        }

        [HttpPost("Save_Import")]
        public ActionResult Post_SaveImport(List<ImportMMS_DM_HangMuc> DH)
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

                    var exit = uow.DM_HangMucs.GetSingle(x => !x.IsDeleted && x.NoiDungBaoDuong.ToLower() == item.NoiDungBaoDuong.ToLower());
                    if (exit == null)
                    {
                        uow.DM_HangMucs.Add(new DM_HangMuc
                        {
                            NoiDungBaoDuong = item.NoiDungBaoDuong,
                            DinhMuc = int.Parse(item.DinhMuc),
                            LoaiBaoDuong = item.LoaiBaoDuong,
                            GhiChu = item.GhiChu,
                            TanSuat_Id = item.TanSuat_Id,
                            CreatedDate = DateTime.Now,
                            CreatedBy = Guid.Parse(User.Identity.Name),
                        });
                    }
                    else
                    {
                        exit.NoiDungBaoDuong = item.NoiDungBaoDuong;
                        exit.DinhMuc = int.Parse(item.DinhMuc);
                        exit.LoaiBaoDuong = item.LoaiBaoDuong;
                        exit.GhiChu = item.GhiChu;
                        exit.TanSuat_Id = item.TanSuat_Id;
                        exit.UpdatedDate = DateTime.Now;
                        exit.UpdatedBy = Guid.Parse(User.Identity.Name);
                        uow.DM_HangMucs.Update(exit);
                    }
                }
                uow.Complete();
                return StatusCode(StatusCodes.Status200OK);
            }
        }
        [HttpGet("FileMau")]
        public ActionResult FileMauTM_DB()
        {
            string fullFilePath = Path.Combine(Directory.GetParent(environment.ContentRootPath).FullName, "Uploads/Templates/FileMau_DM_HangMuc.xlsx");
            string fileName = "FileMau_DM_HangMuc_" + DateTime.Now.ToString("HHmmss") + ".xlsx";
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
        public ActionResult Post(DM_HangMuc data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // if (uow.DM_Loais.Exists(x => x.Name == data.Name && !x.IsDeleted))
                //     return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.Name + " đã tồn tại trong hệ thống");
                // else if (uow.DM_Loais.Exists(x => x.Name == data.Name && x.IsDeleted))
                // {

                //     var d = uow.DM_Loais.GetAll(x => x.Name == data.Name).FirstOrDefault();
                //     d.IsDeleted = false;
                //     d.DeletedBy = null;
                //     d.DeletedDate = null;
                //     d.UpdatedBy = Guid.Parse(User.Identity.Name);
                //     d.UpdatedDate = DateTime.Now;
                //     d.Name = data.Name;
                //     d.Code = data.Code;
                //     d.ThuocNhom = data.ThuocNhom;
                //     d.Nhom_Id = data.Nhom_Id;
                //     uow.DM_Loais.Update(d);

                // }
                // else
                // {
                DM_HangMuc cv = new DM_HangMuc();
                Guid id = Guid.NewGuid();
                cv.Id = id;
                cv.NoiDungBaoDuong = data.NoiDungBaoDuong;
                cv.DinhMuc = data.DinhMuc;
                cv.LoaiBaoDuong = data.LoaiBaoDuong;
                cv.GhiChu = data.GhiChu;
                if (data.TanSuat_Id == Guid.Parse("973a6901-f995-491e-83c5-5ca67d02c294"))
                {
                    data.TanSuat_Id = Guid.Parse("bccd28fb-2e36-4bae-9dbd-ce389d409a95");
                    cv.DinhMuc = cv.DinhMuc * 30;
                }
                cv.TanSuat_Id = data.TanSuat_Id;
                cv.CreatedDate = DateTime.Now;
                cv.CreatedBy = Guid.Parse(User.Identity.Name);
                uow.DM_HangMucs.Add(cv);
                var danhSachPhuongTien = uow.DS_PhuongTiens.GetAll(x => !x.IsDeleted).Select(x => x.Id).ToList();

                // 🔥 Thêm tất cả phương tiện vào ThongTinTheoHangMuc với HangMuc_Id vừa thêm
                foreach (var phuongTienId in danhSachPhuongTien)
                {
                    uow.ThongTinTheoHangMucs.Add(new ThongTinTheoHangMuc
                    {
                        PhuongTien_Id = phuongTienId,
                        HangMuc_Id = cv.Id,
                        CreatedDate = DateTime.Now,
                        CreatedBy = Guid.Parse(User.Identity.Name),
                    });
                }
                // }

                uow.Complete();
                return Ok();
            }
        }

        [HttpPut]
        public ActionResult Put(Guid id, DM_HangMuc duLieu)
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
                uow.DM_HangMucs.Update(duLieu);
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
                DM_HangMuc duLieu = uow.DM_HangMucs.GetById(id);

                if (duLieu == null)
                {
                    return NotFound();
                }
                duLieu.DeletedDate = DateTime.Now;
                duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                duLieu.IsDeleted = true;
                uow.DM_HangMucs.Update(duLieu);
                uow.Complete();
                return Ok(duLieu);
            }

        }

    }
}
