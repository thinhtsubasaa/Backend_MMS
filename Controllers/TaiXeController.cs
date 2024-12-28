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

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TaiXeController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        public TaiXeController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
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
            var data = uow.TaiXes.GetAll(t => !t.IsDeleted && (t.MaTaiXe.ToLower().Contains(keyword.ToLower()) || t.TenTaiXe.ToLower().Contains(keyword.ToLower())), null, include
                ).Select(x => new
                {
                    x.Id,
                    x.MaTaiXe,
                    x.TenTaiXe,
                    x.HangBang,
                    x.SoDienThoai,
                    QrCode = Commons.EnCode(x.MaTaiXe),
                    x.isVaoCong


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
        [HttpGet("TaiXe")]
        public ActionResult GetTaiXe(string keyword)
        {
            if (keyword == null) keyword = "";
            string[] include = { };
            var data = uow.TaiXes.GetAll(t => !t.IsDeleted && (t.MaTaiXe.ToLower().Contains(keyword.ToLower()) || t.TenTaiXe.ToLower().Contains(keyword.ToLower())), null, include
                ).Select(x => new
                {
                    x.Id,
                    x.MaTaiXe,
                    x.TenTaiXe,
                    x.HangBang,
                    x.SoDienThoai,
                    QrCode = Commons.EnCode(x.MaTaiXe),
                    x.isVaoCong

                });
            return Ok(data);
        }



        [HttpPost("CapNhat/{id}")]
        public ActionResult CapNhatTrangThai(Guid id)
        {
            lock (Commons.LockObjectState)
            {
                TaiXe duLieu = uow.TaiXes.GetById(id);
                duLieu.isVaoCong = !duLieu.isVaoCong;
                duLieu.UpdatedBy = Guid.Parse(User.Identity.Name);
                duLieu.UpdatedDate = DateTime.Now;
                uow.TaiXes.Update(duLieu);
                uow.Complete();
                if (duLieu.isVaoCong)
                    return StatusCode(StatusCodes.Status200OK, "Cập nhật Sử dụng thành công");
                else return StatusCode(StatusCodes.Status200OK, "Cập nhật Hủy sử dụng thành công");
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
