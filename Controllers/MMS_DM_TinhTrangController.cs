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
    public class MMS_DM_TinhTrangController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        public MMS_DM_TinhTrangController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
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

            var data = uow.DM_TinhTrangs.GetAll(t => !t.IsDeleted
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
        [HttpGet("TinhTrang")]
        public ActionResult GetTaiXe()
        {
            var data = uow.DM_TinhTrangs.GetAll(t => !t.IsDeleted, x => x.OrderBy(x => x.Arrange)
                ).Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Arrange,
                    x.Note,
                });
            return Ok(data);
        }

        [HttpGet("GetById")]
        public ActionResult Get(Guid id)
        {
            var query = uow.DM_TinhTrangs.GetAll(x => x.Id == id).Select(x => new
            {
                x.Id,
                x.Arrange,
                x.Name,
                x.Note,
            }).FirstOrDefault();
            if (query == null)
            {
                return NotFound();
            }
            return Ok(query);
        }

        [HttpPost]
        public ActionResult Post(DM_TinhTrang data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (uow.DM_TinhTrangs.Exists(x => x.Name == data.Name && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.Name + " đã tồn tại trong hệ thống");
                else if (uow.DM_TinhTrangs.Exists(x => x.Name == data.Name && x.IsDeleted))
                {

                    var d = uow.DM_TinhTrangs.GetAll(x => x.Name == data.Name).FirstOrDefault();
                    d.IsDeleted = false;
                    d.DeletedBy = null;
                    d.DeletedDate = null;
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.Arrange = data.Arrange;
                    d.Name = data.Name;
                    d.Note = data.Note;
                    uow.DM_TinhTrangs.Update(d);

                }
                else
                {
                    DM_TinhTrang cv = new DM_TinhTrang();
                    Guid id = Guid.NewGuid();
                    cv.Id = id;
                    cv.Arrange = data.Arrange;
                    cv.Name = data.Name;
                    cv.Note = data.Note;
                    cv.CreatedDate = DateTime.Now;
                    cv.CreatedBy = Guid.Parse(User.Identity.Name);
                    uow.DM_TinhTrangs.Add(cv);
                }

                uow.Complete();
                return Ok();
            }
        }

        [HttpPut]
        public ActionResult Put(Guid id, DM_TinhTrang duLieu)
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
                uow.DM_TinhTrangs.Update(duLieu);
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
                DM_TinhTrang duLieu = uow.DM_TinhTrangs.GetById(id);

                if (duLieu == null)
                {
                    return NotFound();
                }
                duLieu.DeletedDate = DateTime.Now;
                duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                duLieu.IsDeleted = true;
                uow.DM_TinhTrangs.Update(duLieu);
                uow.Complete();
                return Ok(duLieu);
            }

        }

    }
}
