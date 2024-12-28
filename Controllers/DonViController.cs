using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ERP.Infrastructure;
using ERP.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using static ERP.Data.MyDbContext;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DonViController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        public static IWebHostEnvironment environment;
        public DonViController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment, RoleManager<ApplicationRole> _roleManager)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
            roleManager = _roleManager;
        }

        [HttpGet]
        public ActionResult Get(string keyword, int page = 1, Guid? tapdoanid = null, Guid? donviid = null)
        {
            if (keyword == null) keyword = "";
            var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).ToList();
            string[] include = { "TapDoan" };
            var data = uow.DonVis.GetAll(t => !t.IsDeleted
            && (tapdoanid == null || t.TapDoan_Id == tapdoanid) && (donviid == null || t.Id==donviid)
            && (t.MaDonVi.ToLower().Contains(keyword.ToLower()) || t.TenDonVi.ToLower().Contains(keyword.ToLower())), null, include
                ).Select(x => new
                {
                    x.Id,
                    x.MaDonVi,
                    x.TenDonVi,
                    x.TapDoan.MaTapDoan,
                    x.TapDoan.TenTapDoan,
                    x?.DonVi_Id
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

        [HttpGet("get-by-role")]
        public ActionResult GetByrole(string keyword, int page = 1, Guid? tapdoanid = null, Guid? User_Id = null)
        {
            if (User_Id == null)
            {
                if (keyword == null) keyword = "";
                var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).ToList();
                var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
                var roles = userManager.GetRolesAsync(exit).Result;
                var isAdmin = roles.Contains("Administrator");
                if (isAdmin)
                {
                    string[] include = { "TapDoan" };
                    var data = uow.DonVis.GetAll(t => !t.IsDeleted
                    && (tapdoanid == null || t.TapDoan_Id == tapdoanid)
                    && (t.MaDonVi.ToLower().Contains(keyword.ToLower()) || t.TenDonVi.ToLower().Contains(keyword.ToLower())), null, include
                        ).Select(x => new
                        {
                            x.Id,
                            x.MaDonVi,
                            x.TenDonVi,
                            x.TapDoan.MaTapDoan,
                            x.TapDoan.TenTapDoan,
                            x?.DonVi_Id
                        });
                    if (data == null)
                    {
                        return Ok(data);
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
                    var role = uow.role_DV_PBs.GetAll(x => roleIds.Contains(x.RoleByDonVi_Id)).GroupBy(x => x.DonVi_Id)
        .Select(g => g.First()).ToList();
                    var responseDataList = new List<object>(); // Danh sách dữ liệu từ bảng khác

                    foreach (var rol in role)
                    {
                        string[] include = { "TapDoan" };
                        var data = uow.DonVis.GetAll(t => !t.IsDeleted
                            && (tapdoanid == null || t.TapDoan_Id == tapdoanid)
                            && (role.Count > 0 && t.Id == rol.DonVi_Id)
                            && (t.MaDonVi.ToLower().Contains(keyword.ToLower()) || t.TenDonVi.ToLower().Contains(keyword.ToLower())), null, include
                        ).Select(x => new
                        {
                            x.Id,
                            x.MaDonVi,
                            x.TenDonVi,
                            x.TapDoan.TenTapDoan,
                            x?.DonVi_Id
                        });

                        if (data != null)
                        {
                            responseDataList.AddRange(data.Distinct(new DataEqualityComparer()));

                        }
                    }

                    if (responseDataList.Count == 0)
                    {
                        return Ok(responseDataList);
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
            else
            {
                if (keyword == null) keyword = "";
                var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).ToList();
                var exit = userManager.FindByIdAsync(User_Id.ToString()).Result;
                var roles = userManager.GetRolesAsync(exit).Result;
                var isAdmin = roles.Contains("Administrator");
                if (isAdmin)
                {
                    string[] include = { "TapDoan" };
                    var data = uow.DonVis.GetAll(t => !t.IsDeleted
                    && (tapdoanid == null || t.TapDoan_Id == tapdoanid)
                    && (t.MaDonVi.ToLower().Contains(keyword.ToLower()) || t.TenDonVi.ToLower().Contains(keyword.ToLower())), null, include
                        ).Select(x => new
                        {
                            x.Id,
                            x.MaDonVi,
                            x.TenDonVi,
                            x.TapDoan.MaTapDoan,
                            x.TapDoan.TenTapDoan,
                            x?.DonVi_Id
                        });
                    if (data == null)
                    {
                        return Ok(data);
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
                    var appUser = userManager.FindByIdAsync(User_Id.ToString()).Result;
                    var roledv = uow.roleByDonVis.GetAll(x => !x.IsDeleted && x.User_Id == appUser.Id).ToList();
                    var roleIds = roledv.Select(rv => rv.Id).ToList();
                    var role = uow.role_DV_PBs.GetAll(x => roleIds.Contains(x.RoleByDonVi_Id)).GroupBy(x => x.DonVi_Id)
        .Select(g => g.First()).ToList();
                    var responseDataList = new List<object>(); // Danh sách dữ liệu từ bảng khác

                    foreach (var rol in role)
                    {
                        string[] include = { "TapDoan" };
                        var data = uow.DonVis.GetAll(t => !t.IsDeleted
                            && (tapdoanid == null || t.TapDoan_Id == tapdoanid)
                            && (role.Count > 0 && t.Id == rol.DonVi_Id)
                            && (t.MaDonVi.ToLower().Contains(keyword.ToLower()) || t.TenDonVi.ToLower().Contains(keyword.ToLower())), null, include
                        ).Select(x => new
                        {
                            x.Id,
                            x.MaDonVi,
                            x.TenDonVi,
                            x.TapDoan.TenTapDoan,
                            x?.DonVi_Id
                        });

                        if (data != null)
                        {
                            responseDataList.AddRange(data.Distinct(new DataEqualityComparer()));

                        }
                    }

                    if (responseDataList.Count == 0)
                    {
                        return Ok(responseDataList);
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

        }

        public class DataEqualityComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y)
            {
                // So sánh các trường dữ liệu để xác định dòng trùng lặp
                var xProps = x.GetType().GetProperties();
                var yProps = y.GetType().GetProperties();

                foreach (var prop in xProps)
                {
                    var xValue = prop.GetValue(x);
                    var yValue = yProps.FirstOrDefault(p => p.Name == prop.Name)?.GetValue(y);

                    if (!Equals(xValue, yValue))
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode(object obj)
            {
                // Sử dụng GetHashCode của các trường dữ liệu để tính mã băm
                var props = obj.GetType().GetProperties();
                var hashCode = new HashCode();

                foreach (var prop in props)
                {
                    var value = prop.GetValue(obj);
                    hashCode.Add(value);
                }

                return hashCode.ToHashCode();
            }
        }


        [HttpGet("{id}")]
        public ActionResult Get(Guid id)
        {
            var query = uow.DonVis.GetAll(x => x.Id == id).Select(x => new
            {
                x.Id,
                x.MaDonVi,
                x.TenDonVi,
                x.TapDoan_Id,
                x.DonVi_Id
            }).FirstOrDefault();
            if (query == null)
            {
                return NotFound();
            }
            return Ok(query);
        }

        [HttpPost]
        public ActionResult Post(ClassDonVi data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (uow.DonVis.Exists(x => x.MaDonVi == data.MaDonVi && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.MaDonVi + " đã tồn tại trong hệ thống");
                else if (uow.DonVis.Exists(x => x.MaDonVi == data.MaDonVi && x.IsDeleted))
                {
                    var t = uow.DonVis.GetAll(x => x.MaDonVi == data.MaDonVi).FirstOrDefault();
                    var max_thutu = uow.DonVis.GetAll(x => x.Id == data.DonVi_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    t.IsDeleted = false;
                    t.DeletedBy = null;
                    t.DeletedDate = null;
                    t.UpdatedBy = Guid.Parse(User.Identity.Name);
                    t.UpdatedDate = DateTime.Now;
                    t.MaDonVi = data.MaDonVi;
                    t.TenDonVi = data.TenDonVi;
                    t.TapDoan_Id = data.TapDoan_Id;
                    t.DonVi_Id = data.DonVi_Id;
                    t.ThuTu = max_thutu + 1;
                    uow.DonVis.Update(t);

                }
                else
                {
                    var max_thutu = uow.DonVis.GetAll(x => x.Id == data.DonVi_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    DonVi dv = new DonVi();
                    Guid id = Guid.NewGuid();
                    dv.Id = id;
                    dv.MaDonVi = data.MaDonVi;
                    dv.TenDonVi = data.TenDonVi;
                    dv.TapDoan_Id = data.TapDoan_Id;
                    dv.CreatedDate = DateTime.Now;
                    dv.CreatedBy = Guid.Parse(User.Identity.Name);
                    dv.DonVi_Id = data.DonVi_Id;
                    dv.ThuTu = max_thutu + 1;
                    uow.DonVis.Add(dv);
                }

                uow.Complete();
                return Ok();
            }
        }

        [HttpPut("{id}")]
        public ActionResult Put(Guid id, DonVi data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // if (id != data.Id)
                // {
                //     return BadRequest();
                // }
                // if (uow.DonVis.Exists(x => x.MaDonVi == data.MaDonVi && x.Id != data.Id && !x.IsDeleted))
                //     return StatusCode(StatusCodes.Status409Conflict, "Mã " + data.MaDonVi + " đã tồn tại trong hệ thống");
                // else 
                if (uow.DonVis.Exists(x => x.MaDonVi == data.MaDonVi && x.IsDeleted))
                {
                    var t = uow.DonVis.GetAll(x => x.Id == data.Id).FirstOrDefault();
                    var max_thutu = uow.DonVis.GetAll(x => x.Id == data.DonVi_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    t.IsDeleted = false;
                    t.DeletedBy = null;
                    t.DeletedDate = null;
                    t.UpdatedBy = Guid.Parse(User.Identity.Name);
                    t.UpdatedDate = DateTime.Now;
                    t.MaDonVi = data.MaDonVi;
                    t.TenDonVi = data.TenDonVi;
                    t.TapDoan_Id = data.TapDoan_Id;
                    t.DonVi_Id = data.DonVi_Id;
                    t.DonVi_Id = data.DonVi_Id;
                    uow.DonVis.Update(t);
                }
                else
                {
                    var t = uow.DonVis.GetAll(x => x.Id == id).FirstOrDefault();
                    var max_thutu = uow.DonVis.GetAll(x => x.Id == data.DonVi_Id && !x.IsDeleted).Max(x => x?.ThuTu) ?? 0;
                    t.UpdatedBy = Guid.Parse(User.Identity.Name);
                    t.UpdatedDate = DateTime.Now;
                    t.MaDonVi = data.MaDonVi;
                    t.TenDonVi = data.TenDonVi;
                    t.TapDoan_Id = data.TapDoan_Id;
                    t.DonVi_Id = data.DonVi_Id;
                    uow.DonVis.Update(t);
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
                DonVi duLieu = uow.DonVis.GetById(id);
                var query = uow.chiTiet_DV_PB_BPs.GetAll(x=>x.DonVi_Id == id);
                if (query.Count() == 0 && !uow.phongbans.Exists(x => x.DonVi_Id == id && !x.IsDeleted))
                {
                    if (duLieu == null)
                    {
                        return NotFound();
                    }
                    duLieu.DeletedDate = DateTime.Now;
                    duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                    duLieu.IsDeleted = true;
                    uow.DonVis.Update(duLieu);
                    uow.Complete();
                    return Ok(duLieu);
                }
                return StatusCode(StatusCodes.Status409Conflict, "Bạn chỉ có thể chỉnh sửa thông tin này");
            }

        }

        [HttpDelete("Remove/{id}")]
        public ActionResult Delete_Remove(Guid id)
        {
            lock (Commons.LockObjectState)
            {
                uow.DonVis.Delete(id);
                uow.Complete();
                return Ok();
            }
        }

        /*        [HttpPost("KiemTraDuLieuImport")]
                public ActionResult KiemTraDuLieuImport(List<ClassDonViImport> data)
                {
                    var tapdoan = uow.tapDoans.GetAll(x => !x.IsDeleted);
                    foreach (var item in data)
                    {
                        item.ClassName = "new";
                        var td = tapdoan.FirstOrDefault(x => x.MaTapDoan.ToLower() == item.MaTapDoan.ToLower());
                        if (td == null)
                        {
                            item.ClassName = "error";
                            item.GhiChuImport += "Mã tập đoàn chưa có trong danh mục ";
                            return StatusCode(StatusCodes.Status409Conflict, item.GhiChuImport);
                        }
                        else item.TapDoan_Id = td.Id;
                        // Kiểm tra mã đơn vị không vượt quá 50 kí tự
                        if (item.MaDonVi.Length > 50)
                        {
                            item.ClassName = "error";
                            item.GhiChuImport += "Mã đơn vị vượt quá giới hạn kí tự cho phép (50 kí tự)";
                            return StatusCode(StatusCodes.Status409Conflict, item.GhiChuImport);
                        }

                        // Kiểm tra tên đơn vị không vượt quá 250 kí tự
                        if (item.TenDonVi.Length > 250)
                        {
                            item.ClassName = "error";
                            item.GhiChuImport += "Tên đơn vị vượt quá giới hạn kí tự cho phép (250 kí tự)";
                            return StatusCode(StatusCodes.Status409Conflict, item.GhiChuImport);
                        }
                        if (uow.DonVis.Exists(x => x.MaDonVi == item.MaDonVi))
                        {
                            item.ClassName = "error";
                            item.GhiChuImport += "Mã đơn vị " + item.MaDonVi + "đã tồn tại trong hệ thống";
                            return StatusCode(StatusCodes.Status409Conflict, item.GhiChuImport);
                        }
                    }
                    return Ok(data);
                }*/


        public class ClassDonViImport
        {
            public Guid Id { get; set; }
            public string MaDonVi { get; set; }
            public string TenDonVi { get; set; }
            public string MaTapDoan { get; set; }
            public string TenTapDoan { get; set; }
            public Guid? TapDoan_Id { get; set; }
            public Guid? DonViCha_Id { get; set; }
            public string MaDonViCha { get; set; }
            public string ClassName { get; set; }
            public string GhiChuImport { get; set; }
        }
        [HttpPost("ImportExel")]
        public ActionResult ImportExel(List<ClassDonViImport> data)
        {
            var tapdoan = uow.tapDoans.GetAll(x => !x.IsDeleted);
            var donvicha = uow.DonVis.GetAll(x => !x.IsDeleted);
            foreach (var item in data)
            {
                item.ClassName = "new";
                var td = tapdoan.FirstOrDefault(x => x.MaTapDoan.ToLower() == item.MaTapDoan.ToLower());
                if (td == null)
                {
                    item.ClassName = "error";
                    item.GhiChuImport += item.MaDonVi + " có mã tập đoàn chưa có trong danh mục ";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }
                else item.TapDoan_Id = td.Id;
                if (!string.IsNullOrEmpty(item.MaDonViCha))
                {
                    item.MaDonViCha = item.MaDonViCha.Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
                    var dvc = donvicha.FirstOrDefault(x => x.MaDonVi.ToLower() == item.MaDonViCha.ToLower());
                    if (dvc == null)
                    {
                        item.ClassName = "error";
                        item.GhiChuImport += item.MaDonViCha + " có mã đơn vị cha chưa có trong danh mục ";
                        return StatusCode(StatusCodes.Status409Conflict, item);
                    }
                    else item.DonViCha_Id = dvc.Id;
                }

                // Kiểm tra mã đơn vị không vượt quá 50 kí tự
                if (item.MaDonVi.Length > 50)
                {
                    item.ClassName = "error";
                    item.GhiChuImport += item.MaDonVi + " có mã đơn vị vượt quá giới hạn kí tự cho phép (50 kí tự)";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }

                // Kiểm tra tên đơn vị không vượt quá 250 kí tự
                if (item.TenDonVi.Length > 250)
                {
                    item.ClassName = "error";
                    item.GhiChuImport += item.MaDonVi + " có tên đơn vị vượt quá giới hạn kí tự cho phép (250 kí tự)";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }
                if (uow.DonVis.Exists(x => x.MaDonVi == item.MaDonVi && !x.IsDeleted))
                {
                    item.ClassName = "error";
                    item.GhiChuImport += "Mã đơn vị " + item.MaDonVi + " đã tồn tại trong hệ thống";
                    return StatusCode(StatusCodes.Status409Conflict, item);
                }
                item.MaDonVi = item.MaDonVi.Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
                item.TenDonVi = item.TenDonVi.Replace("\n", "").Replace("\r", "").Replace("\t", "").Trim();
                if (uow.DonVis.Exists(x => x.MaDonVi == item.MaDonVi && x.IsDeleted))
                {
                    item.ClassName = "renew";
                }
                DonVi dv = new DonVi();
                dv.TapDoan_Id = item.TapDoan_Id;
                dv.MaDonVi = item.MaDonVi;
                dv.TenDonVi = item.TenDonVi;
                dv.DonVi_Id = item.DonViCha_Id;
                var id = Guid.NewGuid();
                if (item.ClassName == "new")
                {
                    dv.Id = id;
                    dv.CreatedBy = Guid.Parse(User.Identity.Name);
                    dv.CreatedDate = DateTime.Now;
                    uow.DonVis.Add(dv);
                }
                else
                {
                    var d = uow.DonVis.GetAll(x => x.MaDonVi == item.MaDonVi && x.IsDeleted).FirstOrDefault();
                    d.TapDoan_Id = item.TapDoan_Id;
                    d.MaDonVi = item.MaDonVi;
                    d.TenDonVi = item.TenDonVi;
                    d.DonVi_Id = item.DonViCha_Id;
                    d.UpdatedBy = Guid.Parse(User.Identity.Name);
                    d.UpdatedDate = DateTime.Now;
                    d.IsDeleted = false;
                    uow.DonVis.Update(d);

                }
            }
            uow.Complete();
            return Ok();
        }

        [HttpPost("ExportFileExcel")]
        public ActionResult ExportFileExcel()
        {
            string fullFilePath = Path.Combine(environment.ContentRootPath, "Uploads/Templates/Danhmucdonvi.xlsx");
            //data[0].NgayBanGiao = string.Format("dd/MM/yyyy");
            //string[] arrDate = data[0].NgayBanGiao.Split("/");
            using (ExcelPackage package = new ExcelPackage(new FileInfo(fullFilePath)))
            {
                if (package.Workbook.Worksheets.Count == 0)
                {
                    // Add a new worksheet if none exists
                    package.Workbook.Worksheets.Add("Sheet1");
                }
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Bảng tập đoàn"]; // Lấy worksheet có tên là "Bảng Tập Đoàn"
                if (worksheet == null)
                {
                    // Tạo worksheet mới nếu không tìm thấy worksheet có tên "Bảng Tập Đoàn"
                    worksheet = package.Workbook.Worksheets.Add("Bảng tập đoàn");
                }
                int stt = 1;
                int sublistIndex = 0;
                int indexrow = 3;
                var tapdoan = uow.tapDoans.GetAll(x => !x.IsDeleted).ToList();
                foreach (var item in tapdoan)
                {
                    worksheet.InsertRow(indexrow, 1, indexrow);
                    worksheet.Row(indexrow).Height = 20;

                    worksheet.Cells["A" + indexrow].Value = stt;
                    worksheet.Cells["B" + indexrow].Value = item.MaTapDoan;
                    worksheet.Cells["C" + indexrow].Value = item.TenTapDoan;
                    for (int col = 1; col <= 3; col++)
                    {
                        var cell = worksheet.Cells[indexrow, col];
                        var border = cell.Style.Border;
                        cell.Style.Font.Name = "Times New Roman";
                        cell.Style.Font.Size = 13;
                        border.BorderAround(ExcelBorderStyle.Thin, Color.Black);
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        if (col >= 1 && col <= 3)
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.WrapText = true;
                        }

                    }
                    var cellA = worksheet.Cells["A" + indexrow];
                    cellA.Style.Font.Name = "Times New Roman";
                    cellA.Style.Font.Size = 13;
                    cellA.Style.WrapText = true;
                    cellA.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Increase the row index
                    stt++;
                    indexrow++;
                    sublistIndex++;

                }
                ExcelWorksheet worksheet2 = package.Workbook.Worksheets["Đơn vị"];
                if (worksheet2 == null)
                {
                    worksheet2 = package.Workbook.Worksheets.Add("Đơn vị");
                }
                int stt2 = 1;
                int sublistIndex2 = 0;
                int indexrow2 = 4;
                string[] include = { "TapDoan" };
                var donvi = uow.DonVis.GetAll(x => !x.IsDeleted, null, include).ToList();
                foreach (var item2 in donvi)
                {
                    worksheet2.InsertRow(indexrow2, 1, indexrow2);
                    worksheet2.Row(indexrow2).Height = 35;

                    worksheet.Cells["A" + indexrow2].Value = stt2;
                    worksheet.Cells["B" + indexrow2].Value = item2.MaDonVi;
                    worksheet.Cells["C" + indexrow2].Value = item2.TenDonVi;
                    worksheet.Cells["D" + indexrow2].Value = item2.TapDoan.TenTapDoan;
                    worksheet.Cells["E" + indexrow].Value = GetTenDonVi(item2?.DonVi_Id);
                    for (int col = 1; col <= 4; col++)
                    {
                        var cell = worksheet2.Cells[indexrow2, col];
                        var border = cell.Style.Border;
                        cell.Style.Font.Name = "Times New Roman";
                        cell.Style.Font.Size = 13;
                        border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        if (col >= 1 && col <= 4)
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            cell.Style.WrapText = true;
                        }

                    }

                    var cellA2 = worksheet2.Cells["A" + indexrow2];
                    cellA2.Style.Font.Name = "Times New Roman";
                    cellA2.Style.Font.Size = 13;
                    cellA2.Style.WrapText = true;
                    cellA2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                    // Increase the row index
                    stt2++;
                    indexrow2++;
                    sublistIndex2++;

                }
                return Ok(new { dataexcel = package.GetAsByteArray() });
            }
        }

        public class DropdownTreeNode_DonVi
        {
            public Guid Id { get; set; }
            public string NameId { get; set; }
            public string MaDonVi { get; set; }
            public string TenDonVi { get; set; }
            public int ThuTu { get; set; }
            public string STT { get; set; }
            public Guid? DonVi_Id { get; set; }
            public Guid? TapDoan_Id { get; set; }
            public string TenTapDoan { get; set; }
            public List<DropdownTreeNode_DonVi> Children { get; set; }
            public bool? Disable { get; set; }
        }
        [HttpGet("don-vi-tree")]
        public ActionResult<List<DropdownTreeNode_DonVi>> GetDonViTree(string keyword,Guid? donviid)
        {
            var rootDonViList = uow.DonVis.GetAll(x => !x.IsDeleted && x.DonVi_Id == null).ToList();
            var result = new List<DropdownTreeNode_DonVi>();
            var topLevelOrder = 1;
            foreach (var rootDonVi in rootDonViList)
            {
                var rootNode = CreateTreeNode(rootDonVi, topLevelOrder.ToString(), rootDonVi.ThuTu, keyword,donviid);
                topLevelOrder++;
                if ((string.IsNullOrEmpty(keyword) || rootNode.MaDonVi.ToLower().Contains(keyword.ToLower()) || rootNode.TenDonVi.ToLower().Contains(keyword.ToLower())) && (donviid==null || rootNode.Id==donviid))
                {
                    result.Add(rootNode);
                }
            }
            return Ok(result);
        }

        private DropdownTreeNode_DonVi CreateTreeNode(DonVi parentDonVi, string parentOrder, int ThuTu,string keyword, Guid? donviid)
        {
            var treeNode = new DropdownTreeNode_DonVi
            {
                Id = parentDonVi.Id,
                MaDonVi = parentDonVi.MaDonVi,
                TenDonVi = parentDonVi.TenDonVi,
                DonVi_Id = parentDonVi.DonVi_Id,
                TapDoan_Id = parentDonVi?.TapDoan_Id,
                TenTapDoan = GetTenTapDoan(parentDonVi?.TapDoan_Id),
                ThuTu = parentDonVi.ThuTu,
                STT = $"{parentOrder}",
                Children = new List<DropdownTreeNode_DonVi>()
            };
            var childOrder = 1;
            var childDonViList = uow.DonVis.GetAll(x => !x.IsDeleted && x.DonVi_Id == parentDonVi.Id).OrderBy(x => x.DonVi_Id).ThenBy(x => x.ThuTu).ToList();
            foreach (var childDonVi in childDonViList)
            {
                var childNode = CreateTreeNode(childDonVi, $"{parentOrder}.{childOrder}", childDonVi.ThuTu,keyword,donviid);
                if ((string.IsNullOrEmpty(keyword) || childNode.MaDonVi.ToLower().Contains(keyword.ToLower()) || childNode.TenDonVi.ToLower().Contains(keyword.ToLower())) && (donviid == null || childNode.Id==donviid))
                {
                    treeNode.Children.Add(childNode);
                }
                childOrder++;
            }

            return treeNode;
        }
        private string GetTenTapDoan(Guid? tapDoanId)
        {
            if (tapDoanId.HasValue)
            {
                var tapDoan = uow.tapDoans.GetById(tapDoanId.Value);
                if (tapDoan != null)
                {
                    return tapDoan.TenTapDoan;
                }
            }
            return null;
        }

        private string GetTenDonVi(Guid? donvi_Id)
        {
            if (donvi_Id.HasValue)
            {
                var donvi = uow.DonVis.GetById(donvi_Id.Value);
                if (donvi != null)
                {
                    return donvi.MaDonVi;
                }
            }
            return null;
        }

    }
}