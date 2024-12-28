using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ERP.Infrastructure;
using ERP.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ERP.Controllers.BoPhanController;
using static ERP.Data.MyDbContext;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChucVuController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        public ChucVuController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
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
            string[] include = { };
            var data = uow.chucVus.GetAll(t => !t.IsDeleted && (t.MaChucVu.ToLower().Contains(keyword.ToLower()) || t.TenChucVu.ToLower().Contains(keyword.ToLower())), null, include
                ).Select(x => new
                {
                    x.Id,
                    x.MaChucVu,
                    x.TenChucVu,

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

        [HttpGet("{id}")]
        public ActionResult Get(Guid id)
        {
            string[] include = { "BoPhan" };
            var duLieu = uow.chucVus.GetAll(x => !x.IsDeleted && x.Id == id, null, null);
            if (duLieu == null)
            {
                return NotFound();
            }
            return Ok(duLieu);
        }

        [HttpPost]
        public ActionResult Post(ClassChucVu data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (uow.chucVus.Exists(x => x.MaChucVu == data.MaChucVu && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.MaChucVu + " đã tồn tại trong hệ thống");
                else if (uow.chucVus.Exists(x => x.MaChucVu == data.MaChucVu && x.IsDeleted))
                {

                    var d = uow.chucVus.GetAll(x => x.MaChucVu == data.MaChucVu).FirstOrDefault();
                    d.IsDeleted = false;
                    d.DeletedBy = null;
                    d.DeletedDate = null;
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.MaChucVu = data.MaChucVu;
                    d.TenChucVu = data.TenChucVu;
                    uow.chucVus.Update(d);

                }
                else
                {
                    ChucVu cv = new ChucVu();
                    Guid id = Guid.NewGuid();
                    cv.Id = id;
                    cv.MaChucVu = data.MaChucVu;
                    cv.TenChucVu = data.TenChucVu;
                    cv.CreatedDate = DateTime.Now;
                    cv.CreatedBy = Guid.Parse(User.Identity.Name);
                    uow.chucVus.Add(cv);
                }

                uow.Complete();
                return Ok();
            }
        }

        [HttpPut("{id}")]
        public ActionResult Put(Guid id, ClassChucVu data)
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
                if (uow.chucVus.Exists(x => x.MaChucVu == data.MaChucVu && x.Id != data.Id && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.MaChucVu + " đã tồn tại trong hệ thống");
                else if (uow.chucVus.Exists(x => x.MaChucVu == data.MaChucVu && x.IsDeleted))
                {
                    var d = uow.chucVus.GetAll(x => x.MaChucVu == data.MaChucVu).FirstOrDefault();
                    d.IsDeleted = false;
                    d.DeletedBy = null;
                    d.DeletedDate = null;
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.MaChucVu = data.MaChucVu;
                    d.TenChucVu = data.TenChucVu;
                    uow.chucVus.Update(d);
                }
                else
                {
                    var d = uow.chucVus.GetAll(x => x.Id == id).FirstOrDefault();
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.MaChucVu = data.MaChucVu;
                    d.TenChucVu = data.TenChucVu;
                    uow.chucVus.Update(d);
                }

                uow.Complete();
                return StatusCode(StatusCodes.Status204NoContent);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
            lock (Commons.LockObjectState)
            {
                ChucVu duLieu = uow.chucVus.GetById(id);
                if (duLieu == null)
                {
                    return NotFound();
                }
                duLieu.DeletedDate = DateTime.Now;
                duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                duLieu.IsDeleted = true;
                uow.chucVus.Update(duLieu);
                uow.Complete();
                return Ok(duLieu);
            }

        }
        [HttpDelete("Remove/{id}")]
        public ActionResult Delete_Remove(Guid id)
        {
            lock (Commons.LockObjectState)
            {
                uow.chucVus.Delete(id);
                uow.Complete();
                return Ok();
            }
        }

        /*        [HttpPost("KiemTraDuLieuImport")]
                public ActionResult KiemTraDuLieuImport(List<ClassChucVuImport> data)
                {
                    var phongban = uow.phongbans.GetAll(x => !x.IsDeleted);
                    foreach (var item in data)
                    {
                        item.ClassName = "new";
                        // Kiểm tra mã chức vụ không vượt quá 50 kí tự
                        if (item.MaChucVu.Length > 50)
                        {
                            item.ClassName = "error";
                            item.GhiChuImport += "Mã chức vụ vượt quá giới hạn kí tự cho phép (50 kí tự)";
                            return StatusCode(StatusCodes.Status409Conflict, item.GhiChuImport);
                        }

                        // Kiểm tra tên chức vụ không vượt quá 250 kí tự
                        if (item.TenChucVu.Length > 250)
                        {
                            item.ClassName = "error";
                            item.GhiChuImport += "Tên chức vụ vượt quá giới hạn kí tự cho phép (250 kí tự)";
                            return StatusCode(StatusCodes.Status409Conflict, item.GhiChuImport);
                        }
                        if (uow.chucVus.Exists(x => x.MaChucVu == item.MaChucVu))
                        {
                            item.ClassName = "error";
                            item.GhiChuImport += "Mã chức vụ " + item.MaChucVu + "đã tồn tại trong hệ thống";
                            return StatusCode(StatusCodes.Status409Conflict, item.GhiChuImport);
                        }
                    }
                    return Ok(data);
                }*/

        public class ClassChucVuImport
        {
            public Guid Id { get; set; }
            public string MaChucVu { get; set; }
            public string TenChucVu { get; set; }
            public string ClassName { get; set; }
            public string GhiChuImport { get; set; }
        }
        [HttpPost("ImportExel")]
        public ActionResult ImportExel(List<ClassChucVuImport> data)
        {
            var phongban = uow.phongbans.GetAll(x => !x.IsDeleted);
            foreach (var item in data)
            {
                item.ClassName = "new";
                // Kiểm tra mã chức vụ không vượt quá 50 kí tự
                if (item.MaChucVu.Length > 50)
                {
                    item.ClassName = "error";
                    item.GhiChuImport = "Mã chức vụ vượt quá giới hạn kí tự cho phép (50 kí tự)";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }

                // Kiểm tra tên chức vụ không vượt quá 250 kí tự
                if (item.TenChucVu.Length > 250)
                {
                    item.ClassName = "error";
                    item.GhiChuImport = "Tên chức vụ vượt quá giới hạn kí tự cho phép (250 kí tự)";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }
                if (uow.chucVus.Exists(x => x.MaChucVu == item.MaChucVu && !x.IsDeleted))
                {
                    item.ClassName = "error";
                    item.GhiChuImport = "Mã chức vụ " + item.MaChucVu + " đã tồn tại trong hệ thống";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }
                if (uow.chucVus.Exists(x => x.MaChucVu == item.MaChucVu && x.IsDeleted))
                {
                    item.ClassName = "renew";
                }

                ChucVu dv = new ChucVu();
                dv.MaChucVu = item.MaChucVu;
                dv.TenChucVu = item.TenChucVu;

                var id = Guid.NewGuid();
                if (item.ClassName == "new")
                {
                    dv.Id = id;
                    dv.CreatedBy = Guid.Parse(User.Identity.Name);
                    dv.CreatedDate = DateTime.Now;
                    uow.chucVus.Add(dv);
                }
                else
                {
                    var cv = uow.chucVus.GetAll(x => x.MaChucVu == item.MaChucVu && x.IsDeleted).FirstOrDefault();
                    cv.MaChucVu = item.MaChucVu;
                    cv.TenChucVu = item.TenChucVu;
                    cv.UpdatedBy = Guid.Parse(User.Identity.Name);
                    cv.UpdatedDate = DateTime.Now;
                    cv.IsDeleted = false;
                    uow.chucVus.Update(cv);

                }
            }
            uow.Complete();
            return Ok();
        }

        [HttpPost("ExportFileExcel")]
        public ActionResult ExportFileExcel()
        {
            string fullFilePath = Path.Combine(environment.ContentRootPath, "Uploads/Templates/Danhmucchucvu.xlsx");
            //data[0].NgayBanGiao = string.Format("dd/MM/yyyy");
            //string[] arrDate = data[0].NgayBanGiao.Split("/");
            using (ExcelPackage package = new ExcelPackage(new FileInfo(fullFilePath)))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    // Add a new worksheet if none exists
                    package.Workbook.Worksheets.Add("Sheet1");
                }
                return Ok(new { dataexcel = package.GetAsByteArray() });
            }
        }
    }
}
