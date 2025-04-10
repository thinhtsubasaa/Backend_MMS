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
    public class MMS_DM_BaoDuongController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        public MMS_DM_BaoDuongController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
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

            var data = uow.DM_Nhoms.GetAll(t => !t.IsDeleted
                ).Select(x => new
                {
                    x.Id,
                    x.Name
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


        [HttpGet("BaoDuong")]
        public ActionResult Get()
        {
            var data = uow.DM_BaoDuongs.GetAll(t => !t.IsDeleted
                ).Select(x => new
                {
                    x.Id,
                    x.LoaiBaoDuong,
                    x.TanSuat,
                    x.GiaTri
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
                    var list_datas = new List<ImportMMS_DM_BaoDuong>();
                    for (int i = 2; i <= rowCount; i++)
                    {
                        if (worksheet.Cells[i, 1].Value == null)
                        {
                            // Nếu không có dữ liệu, dừng vòng lặp
                            break;
                        }

                        object Loai = worksheet.Cells[i, 1].Value;

                        object TanSuat = worksheet.Cells[i, 2].Value;
                        object GiaTri = worksheet.Cells[i, 3].Value;

                        DateTime baseDate = new DateTime(1900, 1, 1);
                        int excelDate = 45329;
                        var lst_Lois = new List<string>();
                        var info = new ImportMMS_DM_BaoDuong();
                        info.Id = Guid.NewGuid();
                        info.LoaiBD = Loai?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.TanSuat = TanSuat?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.GiaTri = GiaTri?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";






                        if (string.IsNullOrEmpty(info.LoaiBD))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Loại BD không được để trống");
                        }
                        // else
                        // {
                        //     var exit = lst_Datas.Where(a => a.SoKhung.ToLower().Trim() == info.SoKhung.ToLower().Trim()).FirstOrDefault();
                        //     if (exit != null)
                        //     {
                        //         info.Id = exit.Id;
                        //     }
                        // }

                        info.lst_Lois = lst_Lois;
                        list_datas.Add(info);
                    }

                    list_datas = list_datas.OrderBy(x => x.IsLoi).ToList();
                    //Kiểm tra trùng dữ liệu khi không có lỗi đầu vào
                    if (list_datas.Where(x => x.IsLoi).Count() == 0)
                    {


                        foreach (var item in list_datas)
                        {
                            if (!string.IsNullOrEmpty(item.LoaiBD))
                            {
                                if (DuplicateSoKhungs(list_datas.Where(x => x.LoaiBD == item.LoaiBD).ToList()) > 0)
                                {
                                    item.IsLoi = true;
                                    item.lst_Lois.Add($"Loại {item.LoaiBD} bị trùng lặp");
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
        private int DuplicateSoKhungs(List<ImportMMS_DM_BaoDuong> lst)
        {
            return lst.GroupBy(p => new { p.LoaiBD }).Where(p => p.Count() > 1).Count();
        }

        [HttpPost("Save_Import")]
        public ActionResult Post_SaveImport(List<ImportMMS_DM_BaoDuong> DH)
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


                    var exit = uow.DM_BaoDuongs.GetSingle(x => !x.IsDeleted && x.LoaiBaoDuong.ToLower() == item.LoaiBD.ToLower());
                    if (exit == null)
                    {
                        uow.DM_BaoDuongs.Add(new DM_BaoDuong
                        {
                            LoaiBaoDuong = item.LoaiBD,
                            TanSuat = item.TanSuat,
                            GiaTri = item.GiaTri,
                            CreatedDate = DateTime.Now,
                            CreatedBy = Guid.Parse(User.Identity.Name),
                        });
                    }
                    else
                    {
                        // var exit = uow.KeHoachGiaoXes.GetSingle(x => x.SoKhung.ToLower() == item.SoKhung.ToLower());
                        exit.LoaiBaoDuong = item.LoaiBD;
                        exit.TanSuat = item.TanSuat;
                        exit.GiaTri = item.GiaTri;
                        exit.UpdatedDate = DateTime.Now;
                        exit.UpdatedBy = Guid.Parse(User.Identity.Name);
                        uow.DM_BaoDuongs.Update(exit);
                    }

                }
                uow.Complete();
                return StatusCode(StatusCodes.Status200OK);
            }
        }

        [HttpGet("GetById")]
        public ActionResult Get(Guid id)
        {
            var query = uow.DM_BaoDuongs.GetAll(x => x.Id == id).Select(x => new
            {
                x.Id,
                x.LoaiBaoDuong,
                x.GiaTri,
                x.TanSuat,
            }).FirstOrDefault();
            if (query == null)
            {
                return NotFound();
            }
            return Ok(query);
        }

        [HttpPost]
        public ActionResult Post(DM_BaoDuong data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (uow.DM_BaoDuongs.Exists(x => x.LoaiBaoDuong == data.LoaiBaoDuong && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.LoaiBaoDuong + " đã tồn tại trong hệ thống");
                else if (uow.DM_BaoDuongs.Exists(x => x.LoaiBaoDuong == data.LoaiBaoDuong && x.IsDeleted))
                {

                    var d = uow.DM_BaoDuongs.GetAll(x => x.LoaiBaoDuong == data.LoaiBaoDuong).FirstOrDefault();
                    d.IsDeleted = false;
                    d.DeletedBy = null;
                    d.DeletedDate = null;
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.LoaiBaoDuong = data.LoaiBaoDuong;
                    d.TanSuat = data.TanSuat;
                    d.GiaTri = data.GiaTri;

                    uow.DM_BaoDuongs.Update(d);

                }
                else
                {
                    DM_BaoDuong cv = new DM_BaoDuong();
                    Guid id = Guid.NewGuid();
                    cv.Id = id;
                    cv.LoaiBaoDuong = data.LoaiBaoDuong;
                    cv.GiaTri = data.GiaTri;
                    cv.TanSuat = data.TanSuat;
                    cv.CreatedDate = DateTime.Now;
                    cv.CreatedBy = Guid.Parse(User.Identity.Name);
                    uow.DM_BaoDuongs.Add(cv);
                }

                uow.Complete();
                return Ok();
            }
        }

        [HttpPut]
        public ActionResult Put(Guid id, DM_BaoDuong duLieu)
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
                uow.DM_BaoDuongs.Update(duLieu);
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
                DM_BaoDuong duLieu = uow.DM_BaoDuongs.GetById(id);

                if (duLieu == null)
                {
                    return NotFound();
                }
                duLieu.DeletedDate = DateTime.Now;
                duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                duLieu.IsDeleted = true;
                uow.DM_BaoDuongs.Update(duLieu);
                uow.Complete();
                return Ok(duLieu);
            }

        }

    }
}
