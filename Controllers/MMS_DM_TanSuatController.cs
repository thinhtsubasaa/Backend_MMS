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
    public class MMS_DM_TanSuatController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        public MMS_DM_TanSuatController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
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
        [HttpGet("TanSuat")]
        public ActionResult Get()
        {
            var data = uow.DM_TanSuats.GetAll(t => !t.IsDeleted
                ).Select(x => new
                {
                    x.Id,
                    x.MaTanSuat,
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
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[8];
                    int rowCount = worksheet.Dimension.Rows;
                    var list_datas = new List<ImportMMS_DM_Nhom>();
                    for (int i = 2; i <= rowCount; i++)
                    {
                        if (worksheet.Cells[i, 1].Value == null)
                        {
                            // Nếu không có dữ liệu, dừng vòng lặp
                            break;
                        }
                        object Name = worksheet.Cells[i, 1].Value;
                        object Note = worksheet.Cells[i, 4].Value;

                        DateTime baseDate = new DateTime(1900, 1, 1);
                        int excelDate = 45329;
                        var lst_Lois = new List<string>();
                        var info = new ImportMMS_DM_Nhom();
                        info.Id = Guid.NewGuid();
                        info.Name = Name?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.Note = Note?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";

                        if (string.IsNullOrEmpty(info.Name))
                        {
                            info.IsLoi = true;
                            lst_Lois.Add("Mã phương tiện không được để trống");
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
        private int DuplicateSoKhungs(List<ImportMMS_DM_Nhom> lst)
        {
            return lst.GroupBy(p => new { p.Name }).Where(p => p.Count() > 1).Count();
        }

        [HttpPost("Save_Import")]
        public ActionResult Post_SaveImport(List<ImportMMS_DM_Nhom> DH)
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


                    var exit = uow.DM_Nhoms.GetSingle(x => !x.IsDeleted && x.Name.ToLower() == item.Name.ToLower());
                    if (exit == null)
                    {
                        uow.DM_Nhoms.Add(new DM_Nhom
                        {
                            Name = item.Name,
                            Note = item.Note,
                            CreatedDate = DateTime.Now,
                            CreatedBy = Guid.Parse(User.Identity.Name),
                        });
                    }
                    else
                    {
                        // var exit = uow.KeHoachGiaoXes.GetSingle(x => x.SoKhung.ToLower() == item.SoKhung.ToLower());
                        exit.Name = item.Name;
                        exit.Note = item.Note;
                        exit.UpdatedDate = DateTime.Now;
                        exit.UpdatedBy = Guid.Parse(User.Identity.Name);
                        uow.DM_Nhoms.Update(exit);
                    }

                }
                uow.Complete();
                return StatusCode(StatusCodes.Status200OK);
            }
        }
        [HttpGet("FileMau")]
        public ActionResult FileMauTM_DB()
        {
            string fullFilePath = Path.Combine(Directory.GetParent(environment.ContentRootPath).FullName, "Uploads/Templates/FileMau_DM_Nhom.xlsx");
            string fileName = "FileMau_DM_Nhom_" + DateTime.Now.ToString("HHmmss") + ".xlsx";
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
            var query = uow.DM_TanSuats.GetAll(x => x.Id == id).Select(x => new
            {
                x.Id,
                x.MaTanSuat,
                x.TanSuat,
                x.GiaTri,
            }).FirstOrDefault();
            if (query == null)
            {
                return NotFound();
            }
            return Ok(query);
        }

        [HttpPost]
        public ActionResult Post(DM_TanSuat data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // if (uow.DM_TanSuats.Exists(x => x.MaTanSuat == data.MaTanSuat && !x.IsDeleted))
                //     return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.Name + " đã tồn tại trong hệ thống");
                // else if (uow.DM_Nhoms.Exists(x => x.Name == data.Name && x.IsDeleted))
                // {

                //     var d = uow.DM_Nhoms.GetAll(x => x.Name == data.Name).FirstOrDefault();
                //     d.IsDeleted = false;
                //     d.DeletedBy = null;
                //     d.DeletedDate = null;
                //     d.UpdatedBy = Guid.Parse(User.Identity.Name);
                //     d.UpdatedDate = DateTime.Now;
                //     d.Name = data.Name;
                //     d.Note = data.Note;
                //     uow.DM_Nhoms.Update(d);

                // }
                // else
                // {
                DM_TanSuat cv = new DM_TanSuat();
                Guid id = Guid.NewGuid();
                cv.Id = id;
                cv.MaTanSuat = data.MaTanSuat;
                cv.TanSuat = data.TanSuat;
                cv.GiaTri = data.GiaTri;
                cv.CreatedDate = DateTime.Now;
                cv.CreatedBy = Guid.Parse(User.Identity.Name);
                uow.DM_TanSuats.Add(cv);
                // }

                uow.Complete();
                return Ok();
            }
        }

        [HttpPut]
        public ActionResult Put(Guid id, DM_TanSuat duLieu)
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
                uow.DM_TanSuats.Update(duLieu);
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
                DM_TanSuat duLieu = uow.DM_TanSuats.GetById(id);

                if (duLieu == null)
                {
                    return NotFound();
                }
                duLieu.DeletedDate = DateTime.Now;
                duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                duLieu.IsDeleted = true;
                uow.DM_TanSuats.Update(duLieu);
                uow.Complete();
                return Ok(duLieu);
            }

        }

    }
}
