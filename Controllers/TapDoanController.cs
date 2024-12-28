using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ERP.Infrastructure;
using ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static ERP.Controllers.DonViController;
using static ERP.Data.MyDbContext;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TapDoanController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        public TapDoanController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
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


            var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
            var roles = userManager.GetRolesAsync(exit).Result;
            var isAdmin = roles.Contains("Administrator");
            if (isAdmin)
            {
                var data = uow.tapDoans.GetAll(t => !t.IsDeleted
                && (t.MaTapDoan.ToLower().Contains(keyword.ToLower()) || t.TenTapDoan.ToLower().Contains(keyword.ToLower()))
                    ).Select(x => new
                    {
                        x.Id,
                        x.MaTapDoan,
                        x.TenTapDoan,
                        x.TapDoan_Id
                    });
                if (data == null)
                {
                    return NotFound();
                }
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
            else
            {
                var appUser = userManager.FindByIdAsync(User.Identity.Name).Result;
                var roledv = uow.roleByDonVis.GetAll(x => !x.IsDeleted && x.User_Id == appUser.Id).ToList();
                var roleIds = roledv.Select(rv => rv.Id).ToList();
                var role = uow.role_DV_PBs.GetAll(x => roleIds.Contains(x.RoleByDonVi_Id)).GroupBy(x => x.TapDoan_Id).Select(g => g.First()).ToList();
                var responseDataList = new List<object>();

                foreach (var rol in role)
                {
                    var data = uow.tapDoans.GetAll(t => !t.IsDeleted
                    && (t.MaTapDoan.ToLower().Contains(keyword.ToLower()) || t.TenTapDoan.ToLower().Contains(keyword.ToLower()))
                        ).Select(x => new
                        {
                            x.Id,
                            x.MaTapDoan,
                            x.TenTapDoan,
                            x.TapDoan_Id
                        });

                    if (data != null)
                    {
                        responseDataList.AddRange(data.Distinct(new DataEqualityComparer()));
                    }
                }

                if (responseDataList.Count == 0)
                {
                    return NotFound();
                }

                if (page == -1)
                {
                    return Ok(responseDataList);
                }
                else
                {
                    int totalRow = responseDataList.Count();
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

                    var datalist = responseDataList.Skip((page - 1) * pageSize).Take(pageSize);
                    return Ok(new
                    {
                        totalRow,
                        totalPage,
                        pageSize,
                        datalist
                    });
                }
            }
        }

        [HttpGet("{id}")]
        public ActionResult Get(Guid id)
        {
            var query = uow.tapDoans.GetAll(x => x.Id == id).Select(x => new
            {
                x.Id,
                x.MaTapDoan,
                x.TenTapDoan,
                x.TapDoan_Id
            }).FirstOrDefault();
            if (query == null)
            {
                return NotFound();
            }
            return Ok(query);
        }

        [HttpPost]
        public ActionResult Post(ClassTapDoan data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (uow.tapDoans.Exists(x => x.MaTapDoan == data.MaTapDoan && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.MaTapDoan + " đã tồn tại trong hệ thống");
                else if (uow.tapDoans.Exists(x => x.MaTapDoan == data.MaTapDoan && x.IsDeleted))
                {
                    var t = uow.tapDoans.GetAll(x => x.MaTapDoan == data.MaTapDoan).FirstOrDefault();
                    var max_thutu = uow.tapDoans.GetAll(x => x.Id == data.TapDoan_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    t.IsDeleted = false;
                    t.DeletedBy = null;
                    t.DeletedDate = null;
                    t.UpdatedBy = Guid.Parse(User.Identity.Name);
                    t.UpdatedDate = DateTime.Now;
                    t.MaTapDoan = data.MaTapDoan;
                    t.TenTapDoan = data.TenTapDoan;
                    t.TapDoan_Id = data.TapDoan_Id;
                    t.ThuTu = max_thutu + 1;
                    uow.tapDoans.Update(t);

                }
                else
                {
                    var max_thutu = uow.tapDoans.GetAll(x => x.Id == data.TapDoan_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    TapDoan td = new TapDoan();
                    Guid id = Guid.NewGuid();
                    td.Id = id;
                    td.MaTapDoan = data.MaTapDoan;
                    td.TenTapDoan = data.TenTapDoan;
                    td.CreatedDate = DateTime.Now;
                    td.CreatedBy = Guid.Parse(User.Identity.Name);
                    td.TapDoan_Id = data.TapDoan_Id;
                    td.ThuTu = max_thutu + 1;
                    uow.tapDoans.Add(td);
                }

                uow.Complete();
                return Ok();
            }
        }

        [HttpPut("{id}")]
        public ActionResult Put(Guid id, ClassTapDoan data)
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
                if (uow.tapDoans.Exists(x => x.MaTapDoan == data.MaTapDoan && x.Id != data.Id && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.MaTapDoan + " đã tồn tại trong hệ thống");
                else if (uow.tapDoans.Exists(x => x.MaTapDoan == data.MaTapDoan && x.IsDeleted))
                {
                    var t = uow.tapDoans.GetAll(x => x.MaTapDoan == data.MaTapDoan).FirstOrDefault();
                    var max_thutu = uow.tapDoans.GetAll(x => x.Id == data.TapDoan_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    t.IsDeleted = false;
                    t.DeletedBy = null;
                    t.DeletedDate = null;
                    t.UpdatedBy = Guid.Parse(User.Identity.Name);
                    t.UpdatedDate = DateTime.Now;
                    t.MaTapDoan = data.MaTapDoan;
                    t.TenTapDoan = data.TenTapDoan;
                    t.TapDoan_Id = data.TapDoan_Id;
                    t.ThuTu = max_thutu + 1;
                    uow.tapDoans.Update(t);
                }
                else
                {
                    var t = uow.tapDoans.GetAll(x => x.Id == id).FirstOrDefault();
                    var max_thutu = uow.tapDoans.GetAll(x => x.Id == data.TapDoan_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    t.UpdatedBy = Guid.Parse(User.Identity.Name);
                    t.UpdatedDate = DateTime.Now;
                    t.MaTapDoan = data.MaTapDoan;
                    t.TenTapDoan = data.TenTapDoan;
                    t.TapDoan_Id = data.TapDoan_Id;
                    t.ThuTu = max_thutu + 1;
                    uow.tapDoans.Update(t);
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
                TapDoan duLieu = uow.tapDoans.GetById(id);
                if (duLieu == null)
                {
                    return NotFound();
                }
                var query = uow.chiTiet_DV_PB_BPs.GetAll(x => x.TapDoan_Id == id);
                if (query.Count() == 0 && !uow.DonVis.Exists(x => x.TapDoan_Id == id && !x.IsDeleted))
                {
                    duLieu.DeletedDate = DateTime.Now;
                    duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                    duLieu.IsDeleted = true;
                    uow.tapDoans.Update(duLieu);
                    uow.Complete();
                    return Ok(duLieu);
                }
                else
                    return StatusCode(StatusCodes.Status409Conflict, "Bạn chỉ có thể chỉnh sửa thông tin này");
            }

        }
        [HttpDelete("Remove/{id}")]
        public ActionResult Delete_Remove(Guid id)
        {
            lock (Commons.LockObjectState)
            {
                uow.tapDoans.Delete(id);
                uow.Complete();
                return Ok();
            }
        }

        public class DropdownTreeNode_TapDoan
        {
            public Guid Id { get; set; }
            public string NameId { get; set; }
            public string MaTapDoan { get; set; }
            public string TenTapDoan { get; set; }
            public int ThuTu { get; set; }
            public string STT { get; set; }
            public Guid? TapDoan_Id { get; set; }
            public List<DropdownTreeNode_TapDoan> Children { get; set; }
            public bool? Disable { get; set; }
        }
        [HttpGet("tap-doan-tree")]
        public ActionResult<List<DropdownTreeNode_TapDoan>> GetTapDoanTree(string keyword, Guid? donviid)
        {
            var rootTapDoanList = uow.tapDoans.GetAll(x => !x.IsDeleted && x.TapDoan_Id == null).ToList();
            var result = new List<DropdownTreeNode_TapDoan>();
            var topLevelOrder = 1;
            foreach (var rootTapDoan in rootTapDoanList)
            {
                var rootNode = CreateTreeNode(rootTapDoan, topLevelOrder.ToString(), rootTapDoan.ThuTu,keyword,donviid);
                topLevelOrder++;
                if ((donviid == null || uow.DonVis.Exists(x => x.Id == donviid && x.TapDoan_Id == rootNode.Id)) && (string.IsNullOrEmpty(keyword) || rootNode.MaTapDoan.ToLower().Contains(keyword.ToLower()) || rootNode.TenTapDoan.ToLower().Contains(keyword.ToLower())))
                {
                    result.Add(rootNode);
                }
            }
            return Ok(result);
        }

        private DropdownTreeNode_TapDoan CreateTreeNode(TapDoan parentTapDoan, string parentOrder, int ThuTu, string keyword, Guid? donviid)
        {
            var treeNode = new DropdownTreeNode_TapDoan
            {
                Id = parentTapDoan.Id,
                MaTapDoan = parentTapDoan.MaTapDoan,
                TenTapDoan = parentTapDoan.TenTapDoan,
                TapDoan_Id = parentTapDoan.TapDoan_Id,
                ThuTu = parentTapDoan.ThuTu,
                STT = $"{parentOrder}",
                Children = new List<DropdownTreeNode_TapDoan>()
            };
            var childOrder = 1;
            var childTapDoanList = uow.tapDoans.GetAll(x => !x.IsDeleted && x.TapDoan_Id == parentTapDoan.Id).OrderBy(x => x.TapDoan_Id).ThenBy(x => x.ThuTu).ToList();
            foreach (var childTapDoan in childTapDoanList)
            {
                var childNode = CreateTreeNode(childTapDoan, $"{parentOrder}.{childOrder}", childTapDoan.ThuTu,keyword,donviid);
                if ((donviid == null || uow.DonVis.Exists(x=>x.Id==donviid && x.TapDoan_Id == childNode.Id)) && (string.IsNullOrEmpty(keyword) || childNode.MaTapDoan.ToLower().Contains(keyword.ToLower()) || childNode.TenTapDoan.ToLower().Contains(keyword.ToLower())))
                {
                    treeNode.Children.Add(childNode);
                }
                childOrder++;
            }

            return treeNode;
        }

    }
}
