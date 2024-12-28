using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ERP.Infrastructure;
using System.Linq;
using System;
using static ERP.Data.MyDbContext;
using Microsoft.AspNetCore.Http;
using ERP.Models;
using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using static ERP.Controllers.AccountController;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Spreadsheet;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleByDonViController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        public static IWebHostEnvironment environment;
        public RoleByDonViController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, IWebHostEnvironment _environment)
        {
            uow = _uow;
            userManager = _userManager;
            environment = _environment;
        }

        [HttpGet]
        public ActionResult Get(string keyword,int page)
         {
            if (keyword == null) keyword = "";
            var pageSizeData = uow.Configs.GetAll(x => !x.IsDeleted).ToList();
            var donvi = uow.DonVis.GetAll(x => !x.IsDeleted).ToList();
            var bophan = uow.BoPhans.GetAll(x => !x.IsDeleted).ToList();
            string[] include = { "User", "Role_DV_PBs.DonVi", "Role_DV_PBs.Phongban", "Role_DV_PBs.BoPhan" };
            var data = uow.roleByDonVis.GetAll(t => !t.IsDeleted
            , null, include).Select(x => new
            {
                x.Id,
                x.User_Id,
                x.User.FullName,
                Lstrole = x.Role_DV_PBs.Select(y => new
                {
                    y.Id,
                    y?.DonVi_Id,
                    y?.Phongban_Id,
                    y?.BoPhan_Id,
                    y?.TapDoan_Id
                })
            });
            if (page == -1)
            {
                return Ok(data); // Trả về toàn bộ dữ liệu nếu page là -1
            }
            else
            {
                int totalRow = data.Count();
                int pageSize = pageSizeData[0].PageSize;
                int totalPage = (int)Math.Ceiling(totalRow / (double)pageSizeData[0].PageSize);
                var datalist = data.OrderByDescending(a => a.Id).Skip((page - 1) * pageSizeData[0].PageSize).Take(pageSizeData[0].PageSize);
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
            string[] include = { "User", "Role_DV_PBs.DonVi", "Role_DV_PBs.Phongban", "Role_DV_PBs.BoPhan" };
            var duLieu = uow.roleByDonVis.GetAll(x => !x.IsDeleted && x.User_Id == id, null, include).Select(x => new
            {
                x.Id,
                x.User_Id,
                x.User.FullName,
                Lstrole = x.Role_DV_PBs.Select(y => new
                {
                    y.Id,
                    TapDoanId = y?.TapDoan_Id,
                    DonViId = y?.DonVi_Id,
                    PhongBanId = y?.Phongban_Id,
                    BoPhanId = y?.BoPhan_Id,
                    GhepNoiIds = $"{y?.TapDoan_Id}_{y?.DonVi_Id}_{y?.Phongban_Id}_{y?.BoPhan_Id}".Trim('_')
                })
            });

            if (duLieu == null)
            {
                return NotFound();
            }

            var responseData = duLieu.Select(x =>
            {
                return new
                {
                    x.Id,
                    x.User_Id,
                    x.FullName,
                    Lstrole = x.Lstrole.Select(y => new
                    {
                        y.Id,
                        y.DonViId,
                        y.PhongBanId,
                        y.BoPhanId,
                        y.TapDoanId,
                        GhepNoiIds = y.GhepNoiIds
                    })
                };
            });

            return Ok(responseData);
        }




        [HttpPost]
        public ActionResult Post(RoleByDonVi data)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (uow.roleByDonVis.Exists(x => x.User_Id == data.User_Id && !x.IsDeleted))
                {
                    var role = uow.roleByDonVis.GetAll(x => x.User_Id == data.User_Id).ToArray();
                    foreach(var rol in role)
                    {
                        rol.User_Id = data.User_Id;
                        rol.IsDeleted = false;
                        rol.DeletedBy = null;
                        rol.DeletedDate = null;
                        rol.UpdatedBy = Guid.Parse(User.Identity.Name);
                        rol.UpdatedDate = DateTime.Now;
                        uow.roleByDonVis.Update(rol);
                        var lstrole = uow.role_DV_PBs.GetAll(x => x.RoleByDonVi_Id == rol.Id).ToArray();
                        if(lstrole.Count() > 0)
                        {
                            foreach(var item in lstrole)
                            {
                                uow.role_DV_PBs.Delete(item.Id);
                            }
                        }
                        var Lstrole = data.Lstrole;
                        foreach (var lst in Lstrole)
                        {
                            Guid idr = Guid.NewGuid();
                            lst.RoleByDonVi_Id=rol.Id;
                            lst.Id = idr;
                            uow.role_DV_PBs.Add(lst);
                        }
                    }
                }    
                else if (uow.roleByDonVis.Exists(x => x.User_Id == data.User_Id && x.IsDeleted))
                {
                    var role = uow.roleByDonVis.GetAll(x => x.User_Id == data.User_Id).ToArray();
                    foreach (var rol in role)
                    {
                        rol.User_Id = data.User_Id;
                        rol.IsDeleted = false;
                        rol.DeletedBy = null;
                        rol.DeletedDate = null;
                        rol.UpdatedBy = Guid.Parse(User.Identity.Name);
                        rol.UpdatedDate = DateTime.Now;
                        uow.roleByDonVis.Update(rol);
                        var lstrole = uow.role_DV_PBs.GetAll(x => x.RoleByDonVi_Id == rol.Id).ToArray();
                        if (lstrole.Count() > 0)
                        {
                            foreach (var item in lstrole)
                            {
                                uow.role_DV_PBs.Delete(item.Id);
                            }
                        }
                        var Lstrole = data.Lstrole;
                        foreach (var lst in Lstrole)
                        {
                            Guid idr = Guid.NewGuid();
                            lst.RoleByDonVi_Id = rol.Id;
                            lst.Id = idr;
                            uow.role_DV_PBs.Add(lst);
                        }
                    }
                }
                else
                {
                    Guid id = Guid.NewGuid();
                    data.Id = id;
                    data.CreatedDate = DateTime.Now;
                    data.CreatedBy = Guid.Parse(User.Identity.Name);
                    uow.roleByDonVis.Add(data);
                    foreach (var lst in data.Lstrole)
                    {
                        lst.RoleByDonVi_Id = data.Id;
                        uow.role_DV_PBs.Add(lst);
                    }
                }              
            }
            uow.Complete();
            return Ok();
        }

        /*[HttpPut("{id}")]
        public ActionResult Put(Guid id, RoleByDonVi data)
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
                if (uow.roleByDonVis.Exists(x => x.User_Id == data.User_Id && !x.IsDeleted))
                    return StatusCode(StatusCodes.Status409Conflict, "Người dùng " + data.User_Id + " đã tồn tại trong hệ thống");
                else if (uow.roleByDonVis.Exists(x => x.User_Id == data.User_Id && x.IsDeleted))
                {
                    var role = uow.roleByDonVis.GetAll(x => x.Id == data.Id).ToArray();

                    role[0].User_Id = data.User_Id;
                    role[0].IsDeleted = false;
                    role[0].DeletedBy = null;
                    role[0].DeletedDate = null;
                    role[0].UpdatedBy = Guid.Parse(User.Identity.Name);
                    role[0].UpdatedDate = DateTime.Now;
                    uow.roleByDonVis.Update(role[0]);
                    foreach (var item in data.Lstrole)
                    {
                        item.RoleByDonVi_Id = role[0].Id;
                        uow.role_DV_PBs.Add(item);
                    }
                }
                else
                {
                    data.UpdatedBy = Guid.Parse(User.Identity.Name);
                    data.UpdatedDate = DateTime.Now;
                    uow.roleByDonVis.Update(data);
                }

                var Lstrole= data.Lstrole;
                var dataCheck = uow.role_DV_PBs.GetAll(x => x.RoleByDonVi_Id == id).ToList();
                if (dataCheck.Count() > 0)
                {
                    foreach (var item in dataCheck)
                    {
                        if (!Lstrole.Exists(x => x.DonVi_Id == item.DonVi_Id))
                        {
                            uow.role_DV_PBs.Delete(item.Id);
                        }
                    }
                    foreach (var item in Lstrole)
                    {
                        if (!dataCheck.Exists(x => x.DonVi_Id == item.DonVi_Id))
                        {
                            item.RoleByDonVi_Id = id;
                            uow.role_DV_PBs.Add(item);                          
                        }
                    }
                }
                else
                {
                    foreach (var item in Lstrole)
                    {
                        item.RoleByDonVi_Id = id;
                        uow.role_DV_PBs.Add(item);                       
                    }
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
                RoleByDonVi duLieu = uow.roleByDonVis.GetById(id);

                    if (duLieu == null)
                    {
                        return NotFound();
                    }
                    var dataChecktttb = uow.role_DV_PBs.GetAll(x => x.RoleByDonVi_Id == id).ToList();
                    foreach (var item in dataChecktttb)
                    {
                        uow.role_DV_PBs.Delete(item.Id);
                    }
                    duLieu.DeletedDate = DateTime.Now;
                    duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                    duLieu.IsDeleted = true;
                    uow.roleByDonVis.Update(duLieu);
                    uow.Complete();
                    return Ok(duLieu);
            }
        }
        [HttpDelete("Remove/{id}")]
        public ActionResult Delete_Remove(Guid id)
        {
            lock (Commons.LockObjectState)
            {
                uow.roleByDonVis.Delete(id);
                uow.Complete();
                return Ok();
            }
        }*/


        [HttpGet("dropdown-by-role")]
        public IActionResult GetDropdownData(Guid? donviId = null, Guid? phongbanId = null, Guid? bophanId = null)
        {
            var exit = userManager.FindByIdAsync(User.Identity.Name).Result;
            var roles = userManager.GetRolesAsync(exit).Result;
            var isAdmin = roles.Contains("Administrator");
            var tapdoans = uow.tapDoans.GetAll(x => !x.IsDeleted).ToList();
            var result = new List<DropdownTreeNode>(); // Danh sách lưu trữ các đối tượng
            if(isAdmin)
            {
                foreach (var tapdoan in tapdoans)
                {
                    var donviRoot = new DropdownTreeNode
                    {
                        Id = tapdoan.Id,
                        Name = tapdoan.TenTapDoan,
                        NameId = tapdoan.Id.ToString(),
                        Level = 0,
                        Children = new List<DropdownTreeNode>()
                    };

                    if (!donviId.HasValue && !phongbanId.HasValue && !bophanId.HasValue)
                    {
                        // Lấy danh sách đơn vị thuộc tập đoàn
                        var donvis = uow.DonVis.GetAll(x => x.TapDoan_Id == tapdoan.Id && !x.IsDeleted);
                        foreach (var donvi in donvis)
                        {
                            var donviNode = new DropdownTreeNode
                            {
                                Id = donvi.Id,
                                NameId = tapdoan.Id + "_" + donvi.Id,
                                Name = donvi.TenDonVi,
                                Level = 1,
                                Children = new List<DropdownTreeNode>()
                            };

                            // Lấy danh sách phòng ban thuộc đơn vị
                            var phongbans = uow.phongbans.GetAll(x => x.DonVi_Id == donvi.Id && !x.IsDeleted);
                            foreach (var phongban in phongbans)
                            {
                                var phongbanNode = new DropdownTreeNode
                                {
                                    Id = phongban.Id,
                                    NameId = tapdoan.Id + "_" + donvi.Id + "_" + phongban.Id,
                                    Name = phongban.TenPhongBan,
                                    Level = 2,
                                    Children = new List<DropdownTreeNode>(),
                                    Disable = false
                                };

                                // Lấy danh sách bộ phận thuộc phòng ban
                                var bophans = uow.BoPhans.GetAll(x => x.PhongBan_Id == phongban.Id && !x.IsDeleted);
                                foreach (var bophan in bophans)
                                {
                                    var bophanNode = new DropdownTreeNode
                                    {
                                        Id = bophan.Id,
                                        NameId = tapdoan.Id + "_" + donvi.Id + "_" + phongban.Id + "_" + bophan.Id,
                                        Name = bophan.TenBoPhan,
                                        Level = 3,
                                        Children = new List<DropdownTreeNode>(),
                                        Disable = true
                                    };

                                    phongbanNode.Children.Add(bophanNode);
                                }
                                if (phongbanNode.Children.Count == 0) // Check if phòng ban has no bộ phận
                                {
                                    phongbanNode.Disable = true; // Set Disable to false when there are no bộ phận
                                }

                                donviNode.Children.Add(phongbanNode);
                            }

                            donviRoot.Children.Add(donviNode);
                        }

                        result.Add(donviRoot);
                    }

                    if (donviId.HasValue && !phongbanId.HasValue && !bophanId.HasValue)
                    {
                        var donvi = uow.DonVis.GetAll(x => x.Id == donviId).ToList();
                        if (donvi != null)
                        {
                            var donviNode = new DropdownTreeNode
                            {
                                Id = donvi[0].Id,
                                NameId = tapdoan.Id + "_" + donvi[0].Id,
                                Name = donvi[0].TenDonVi,
                                Level = 1,
                                Children = new List<DropdownTreeNode>()
                            };

                            // Lấy danh sách phòng ban thuộc đơn vị
                            var phongbans = uow.phongbans.GetAll(x => x.DonVi_Id == donvi[0].Id && !x.IsDeleted);
                            foreach (var phongban in phongbans)
                            {
                                var phongbanNode = new DropdownTreeNode
                                {
                                    Id = phongban.Id,
                                    NameId = tapdoan.Id + "_" + donvi[0].Id + "_" + phongban.Id,
                                    Name = phongban.TenPhongBan,
                                    Level = 2,
                                    Children = new List<DropdownTreeNode>(),
                                    Disable = false
                                };

                                // Lấy danh sách bộ phận thuộc phòng ban
                                var bophans = uow.BoPhans.GetAll(x => x.PhongBan_Id == phongban.Id && !x.IsDeleted);
                                foreach (var bophan in bophans)
                                {
                                    var bophanNode = new DropdownTreeNode
                                    {
                                        Id = bophan.Id,
                                        NameId = tapdoan.Id + "_" + donvi[0].Id + "_" + phongban.Id + "_" + bophan.Id,
                                        Name = bophan.TenBoPhan,
                                        Level = 3,
                                        Children = new List<DropdownTreeNode>(),
                                        Disable = true
                                    };

                                    phongbanNode.Children.Add(bophanNode);
                                }
                                if (phongbanNode.Children.Count == 0) // Check if phòng ban has no bộ phận
                                {
                                    phongbanNode.Disable = true; // Set Disable to false when there are no bộ phận
                                }
                                donviNode.Children.Add(phongbanNode);
                            }

                            donviRoot.Children.Add(donviNode);
                        }

                        result.Add(donviRoot);
                    }

                    if (donviId.HasValue && phongbanId.HasValue && !bophanId.HasValue)
                    {
                        var donvi = uow.DonVis.GetAll(x => x.Id == donviId).ToList();
                        var phongban = uow.phongbans.GetAll(x => x.Id == phongbanId).ToList();
                        if (donvi != null && phongban != null && phongban[0].DonVi_Id == donvi[0].Id)
                        {
                            var donviNode = new DropdownTreeNode
                            {
                                Id = donvi[0].Id,
                                NameId = tapdoan.Id + "_" + donvi[0].Id,
                                Name = donvi[0].TenDonVi,
                                Level = 1,
                                Children = new List<DropdownTreeNode>()
                            };

                            var phongbanNode = new DropdownTreeNode
                            {
                                Id = phongban[0].Id,
                                NameId = tapdoan.Id + "_" + donvi[0].Id + "_" + phongban[0].Id,
                                Name = phongban[0].TenPhongBan,
                                Level = 2,
                                Children = new List<DropdownTreeNode>(),
                                Disable = false
                            };

                            // Lấy danh sách bộ phận thuộc phòng ban
                            var bophans = uow.BoPhans.GetAll(x => x.PhongBan_Id == phongban[0].Id && !x.IsDeleted);
                            foreach (var bophan in bophans)
                            {
                                var bophanNode = new DropdownTreeNode
                                {
                                    Id = bophan.Id,
                                    NameId = tapdoan.Id + "_" + donvi[0].Id + "_" + phongban[0].Id + "_" + bophan.Id,
                                    Name = bophan.TenBoPhan,
                                    Level = 3,
                                    Children = new List<DropdownTreeNode>(),
                                    Disable = true
                                };

                                phongbanNode.Children.Add(bophanNode);
                            }
                            if (phongbanNode.Children.Count == 0) // Check if phòng ban has no bộ phận
                            {
                                phongbanNode.Disable = true; // Set Disable to false when there are no bộ phận
                            }

                            donviNode.Children.Add(phongbanNode);
                            donviRoot.Children.Add(donviNode);
                        }

                        result.Add(donviRoot);
                    }

                    if (donviId.HasValue && phongbanId.HasValue && bophanId.HasValue)
                    {
                        var donvi = uow.DonVis.GetAll(x => x.Id == donviId).ToList();
                        var phongban = uow.phongbans.GetAll(x => x.Id == phongbanId).ToList();
                        var bophan = uow.BoPhans.GetAll(x => x.Id == bophanId).ToList();
                        if (donvi != null && phongban != null && bophan != null && phongban[0].DonVi_Id == donvi[0].Id && bophan[0].PhongBan_Id == phongban[0].Id)
                        {
                            var donviNode = new DropdownTreeNode
                            {
                                Id = donvi[0].Id,
                                NameId = tapdoan.Id + "_" + donvi[0].Id,
                                Name = donvi[0].TenDonVi,
                                Level = 1,
                                Children = new List<DropdownTreeNode>()
                            };

                            var phongbanNode = new DropdownTreeNode
                            {
                                Id = phongban[0].Id,
                                NameId = tapdoan.Id + "_" + donvi[0].Id + "_" + phongban[0].Id,
                                Name = phongban[0].TenPhongBan,
                                Level = 2,
                                Children = new List<DropdownTreeNode>(),
                                Disable = false
                            };

                            var bophanNode = new DropdownTreeNode
                            {
                                Id = bophan[0].Id,
                                NameId = tapdoan.Id + "_" + donvi[0].Id + "_" + phongban[0].Id + "_" + bophan[0].Id,
                                Name = bophan[0].TenBoPhan,
                                Level = 3,
                                Children = new List<DropdownTreeNode>(),
                                Disable = true
                            };

                            phongbanNode.Children.Add(bophanNode);
                            if (phongbanNode.Children.Count == 0) // Check if phòng ban has no bộ phận
                            {
                                phongbanNode.Disable = true; // Set Disable to false when there are no bộ phận
                            }
                            donviNode.Children.Add(phongbanNode);
                            donviRoot.Children.Add(donviNode);
                        }

                        result.Add(donviRoot);
                    }
                }

                return Ok(result);
            }
            else
            {
                var appUser = userManager.FindByIdAsync(User.Identity.Name).Result;
                var roledv = uow.roleByDonVis.GetAll(x => !x.IsDeleted && x.User_Id == appUser.Id).ToList();
                var roleIds = roledv.Select(rv => rv.Id).ToList();
                var role = uow.role_DV_PBs.GetAll(x => roleIds.Contains(x.RoleByDonVi_Id)).GroupBy(x => new { x.DonVi_Id }).Select(g => g.First()).ToList();
                var role2 = uow.role_DV_PBs.GetAll(x => roleIds.Contains(x.RoleByDonVi_Id)).GroupBy(x => new { x.Phongban_Id }).Select(g => g.First()).ToList();
                var role3 = uow.role_DV_PBs.GetAll(x => roleIds.Contains(x.RoleByDonVi_Id)).GroupBy(x => new { x.BoPhan_Id }).Select(g => g.First()).ToList();
                var tapdoanbyrole = uow.tapDoans.GetAll(x => !x.IsDeleted && (role.Count > 0 && x.Id == role[0].TapDoan_Id)).ToList();
                    foreach (var tapdoan in tapdoanbyrole)
                    {
                        var donviRoot = new DropdownTreeNode
                        {
                            Id = tapdoan.Id,
                            Name = tapdoan.TenTapDoan,
                            NameId = tapdoan.Id.ToString(),
                            Level = 0,
                            Children = new List<DropdownTreeNode>()
                        };

                        if (!donviId.HasValue && !phongbanId.HasValue && !bophanId.HasValue)
                        {
                            foreach(var item in role)
                            {
                                var donvis = uow.DonVis.GetAll(x => x.TapDoan_Id == tapdoan.Id && !x.IsDeleted && (role.Count > 0 && x.Id == item.DonVi_Id));
                                foreach (var donvi in donvis)
                                {
                                    var donviNode = new DropdownTreeNode
                                    {
                                        Id = donvi.Id,
                                        NameId = tapdoan.Id + "_" + donvi.Id,
                                        Name = donvi.TenDonVi,
                                        Level = 1,
                                        Children = new List<DropdownTreeNode>()
                                    };

                                foreach(var pb in role2)
                                {
                                    var phongbans = uow.phongbans.GetAll(x => x.DonVi_Id == donvi.Id && !x.IsDeleted && (role.Count > 0 && x.Id == pb.Phongban_Id));
                                    foreach (var phongban in phongbans)
                                    {                                        
                                        var phongbanNode = new DropdownTreeNode
                                        {
                                            Id = phongban.Id,
                                            NameId = tapdoan.Id + "_" + donvi.Id + "_" + phongban.Id,
                                            Name = phongban.TenPhongBan,
                                            Level = 2,
                                            Children = new List<DropdownTreeNode>(),
                                            Disable = false
                                        };

                                            var bophans = uow.BoPhans.GetAll(x => x.PhongBan_Id == pb.Phongban_Id && !x.IsDeleted);
                                            foreach (var bophan in bophans)
                                            {
                                                var bophanNode = new DropdownTreeNode
                                                {
                                                    Id = bophan.Id,
                                                    NameId = tapdoan.Id + "_" + donvi.Id + "_" + phongban.Id + "_" + bophan.Id,
                                                    Name = bophan.TenBoPhan,
                                                    Level = 3,
                                                    Children = new List<DropdownTreeNode>(),
                                                    Disable = true
                                                };

                                                phongbanNode.Children.Add(bophanNode);
                                            }
                                       
                                        if (phongbanNode.Children.Count == 0)
                                        {
                                            phongbanNode.Disable = true;
                                        }

                                        donviNode.Children.Add(phongbanNode);
                                    
                                } }



                                    donviRoot.Children.Add(donviNode);
                                }

                                
                            }
                        result.Add(donviRoot);
                        }
                    }
                    return Ok(result);


            }

        }
    }


}
