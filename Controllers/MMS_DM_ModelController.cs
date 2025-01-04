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
    public class MMS_DM_ModelController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        public MMS_DM_ModelController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
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

            var data = uow.DM_Models.GetAll(t => !t.IsDeleted
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
        [HttpGet("Loai")]
        public ActionResult GetTaiXe()
        {

            var data = uow.DM_Models.GetAll(t => !t.IsDeleted
                ).Select(x => new
                {
                    x.Id,
                    x.Name,

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
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[10];
                    int rowCount = worksheet.Dimension.Rows;
                    var list_datas = new List<ImportMMS_DM_Model>();
                    for (int i = 2; i <= rowCount; i++)
                    {
                        if (worksheet.Cells[i, 3].Value == null)
                        {
                            // Nếu không có dữ liệu, dừng vòng lặp
                            break;
                        }

                        object Name = worksheet.Cells[i, 2].Value;
                        object Option = worksheet.Cells[i, 3].Value;
                        object Code = worksheet.Cells[i, 4].Value;
                        object Type = worksheet.Cells[i, 6].Value;
                        object KLBT = worksheet.Cells[i, 8].Value;
                        object TTMK = worksheet.Cells[i, 9].Value;
                        object KLTB = worksheet.Cells[i, 10].Value;
                        object KLKT = worksheet.Cells[i, 11].Value;


                        DateTime baseDate = new DateTime(1900, 1, 1);
                        int excelDate = 45329;
                        var lst_Lois = new List<string>();
                        var info = new ImportMMS_DM_Model();
                        info.Id = Guid.NewGuid();
                        info.Name = Name?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.Code = Code?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.Option = Option?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.Type = Type?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.KLBT = KLBT?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.TTMK_KLHH = TTMK?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.KLTB = KLTB?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";
                        info.KLKT = KLKT?.ToString().Trim().Replace("\t", "").Replace("\n", "") ?? "";

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
                                if (DuplicateSoKhungs(list_datas.Where(x => x.Option == item.Option).ToList()) > 0)
                                {
                                    item.IsLoi = true;
                                    item.lst_Lois.Add($"Số Khung {item.Option} bị trùng lặp");
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
        private int DuplicateSoKhungs(List<ImportMMS_DM_Model> lst)
        {
            return lst.GroupBy(p => new { p.Name }).Where(p => p.Count() > 1).Count();
        }

        [HttpPost("Save_Import")]
        public ActionResult Post_SaveImport(List<ImportMMS_DM_Model> DH)
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

                    var exit = uow.DM_Models.GetSingle(x => !x.IsDeleted && x.Code.ToLower() == item.Code.ToLower());
                    if (exit == null)
                    {
                        uow.DM_Models.Add(new DM_Model
                        {
                            Name = item.Name,
                            Option = item.Option,
                            Code = item.Code,
                            Type = item.Type,
                            KLBT = item.KLBT,
                            TTMK_KLHH = item.TTMK_KLHH,
                            KLTB = item.KLTB,
                            KLKT = item.KLKT,
                            CreatedDate = DateTime.Now,
                            CreatedBy = Guid.Parse(User.Identity.Name),
                        });
                    }
                    else
                    {
                        exit.Code = item.Code;
                        exit.Option = item.Option;
                        exit.Name = item.Name;
                        exit.Type = item.Type;
                        exit.KLBT = item.KLBT;
                        exit.TTMK_KLHH = item.TTMK_KLHH;
                        exit.KLTB = item.KLTB;
                        exit.KLKT = item.KLKT;
                        exit.Note = item.Note;
                        exit.UpdatedDate = DateTime.Now;
                        exit.UpdatedBy = Guid.Parse(User.Identity.Name);
                        uow.DM_Models.Update(exit);
                    }

                }
                uow.Complete();
                return StatusCode(StatusCodes.Status200OK);
            }
        }

        [HttpGet("GetById")]
        public ActionResult Get(Guid id)
        {
            var query = uow.TaiXes.GetAll(x => x.Id == id).Select(x => new
            {
                x.Id,
                x.MaTaiXe,
                x.TenTaiXe,
                x.HangBang,
                x.SoDienThoai
            }).FirstOrDefault();
            if (query == null)
            {
                return NotFound();
            }
            return Ok(query);
        }

        [HttpPost]
        public ActionResult Post(TaiXe data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (uow.TaiXes.Exists(x => x.MaTaiXe == data.MaTaiXe && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.MaTaiXe + " đã tồn tại trong hệ thống");
                else if (uow.TaiXes.Exists(x => x.MaTaiXe == data.MaTaiXe && x.IsDeleted))
                {

                    var d = uow.TaiXes.GetAll(x => x.MaTaiXe == data.MaTaiXe).FirstOrDefault();
                    d.IsDeleted = false;
                    d.DeletedBy = null;
                    d.DeletedDate = null;
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.MaTaiXe = data.MaTaiXe;
                    d.TenTaiXe = data.TenTaiXe;
                    d.HangBang = data.HangBang;
                    d.SoDienThoai = data.SoDienThoai;
                    uow.TaiXes.Update(d);

                }
                else
                {
                    TaiXe cv = new TaiXe();
                    Guid id = Guid.NewGuid();
                    cv.Id = id;
                    cv.MaTaiXe = data.MaTaiXe;
                    cv.TenTaiXe = data.TenTaiXe;
                    cv.HangBang = data.HangBang;
                    cv.SoDienThoai = data.SoDienThoai;
                    cv.CreatedDate = DateTime.Now;
                    cv.CreatedBy = Guid.Parse(User.Identity.Name);
                    uow.TaiXes.Add(cv);
                }

                uow.Complete();
                return Ok();
            }
        }

        [HttpPut]
        public ActionResult Put(Guid id, TaiXe data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (id != data.Id)
                {
                    return BadRequest();
                }
                if (uow.TaiXes.Exists(x => x.MaTaiXe == data.MaTaiXe && x.Id != data.Id && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.MaTaiXe + " đã tồn tại trong hệ thống");
                else if (uow.TaiXes.Exists(x => x.MaTaiXe == data.MaTaiXe && x.IsDeleted))
                {

                    var d = uow.TaiXes.GetAll(x => x.MaTaiXe == data.MaTaiXe).FirstOrDefault();
                    d.IsDeleted = false;
                    d.DeletedBy = null;
                    d.DeletedDate = null;
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.MaTaiXe = data.MaTaiXe;
                    d.TenTaiXe = data.TenTaiXe;
                    d.HangBang = data.HangBang;
                    d.SoDienThoai = data.SoDienThoai;
                    uow.TaiXes.Update(d);

                }
                else
                {
                    var d = uow.TaiXes.GetAll(x => x.Id == id).FirstOrDefault();
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.MaTaiXe = data.MaTaiXe;
                    d.TenTaiXe = data.TenTaiXe;
                    d.HangBang = data.HangBang;
                    d.SoDienThoai = data.SoDienThoai;
                    uow.TaiXes.Update(d);
                }

                uow.Complete();
                return StatusCode(StatusCodes.Status204NoContent);
            }
        }

        [HttpDelete]
        public ActionResult Delete(Guid id)
        {
            lock (Commons.LockObjectState)
            {
                TaiXe duLieu = uow.TaiXes.GetById(id);

                if (duLieu == null)
                {
                    return NotFound();
                }
                duLieu.DeletedDate = DateTime.Now;
                duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                duLieu.IsDeleted = true;
                uow.TaiXes.Update(duLieu);
                uow.Complete();
                return Ok(duLieu);
            }

        }

    }
}
