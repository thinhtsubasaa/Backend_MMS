using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ERP.Infrastructure;
using ERP.Models;
using static ERP.Data.MyDbContext;

namespace ERP.Controllers
{
    [EnableCors("CorsApi")]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IUnitofWork uow;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        public MenuController(IUnitofWork _uow, UserManager<ApplicationUser> _userManager, RoleManager<ApplicationRole> _roleManager)
        {
            uow = _uow;
            userManager = _userManager;
            roleManager = _roleManager;
        }

        public class DropdownTreeNode_Menu
        {
            public Guid Id { get; set; }
            public string TenMenu { get; set; }
            public string Icon { get; set; }
            public string Url { get; set; }
            public int Level { get; set; }
            public int ThuTu { get; set; }
            public bool isMoBi { get; set; }
            public string STT { get; set; }
            public Guid? Parent_Id { get; set; }
            public List<DropdownTreeNode_Menu> Children { get; set; }
            public bool IsUsed { get; set; }
            public bool IsRemove { get; set; }
            public Guid? PhanMem_Id { get; set; }
            public Guid? DonVi_id { get; set; }
        }
        [HttpGet("menu-tree")]
        public ActionResult<List<DropdownTreeNode_Menu>> GetMenuTree(Guid? PhanMem_Id, Guid? DonVi_Id, Guid? TapDoan_Id, Guid? PhongBan_Id)
        {
            var rootMenuList = uow.Menus.GetAll(x => !x.IsDeleted && x.Parent_Id == null && x.PhanMem_Id == PhanMem_Id && (DonVi_Id == null || x.DonVi_id == DonVi_Id) && (TapDoan_Id == null || x.TapDoan_Id == TapDoan_Id) && (PhongBan_Id == null || x.PhongBan_Id == PhongBan_Id)).OrderBy(x => x.Parent_Id).ThenBy(x => x.ThuTu).ToList();
            var result = new List<DropdownTreeNode_Menu>();
            var topLevelOrder = 1;
            foreach (var rootmenu in rootMenuList)
            {
                var rootNode = CreateTreeNode(rootmenu, topLevelOrder.ToString(), 0);
                topLevelOrder++;
                result.Add(rootNode);
            }
            return Ok(result);
        }
        [HttpPost("CapNhat/{id}")]
        public ActionResult CapNhatTrangThai(Guid id)
        {
            lock (Commons.LockObjectState)
            {
                Menu duLieu = uow.Menus.GetSingle(x=>x.Id==id);
                duLieu.isMoBi = !duLieu.isMoBi;
                duLieu.UpdatedBy = Guid.Parse(User.Identity.Name);
                duLieu.UpdatedDate = DateTime.Now;
                uow.Menus.Update(duLieu);
                uow.Complete();
                if (duLieu.isMoBi)
                    return StatusCode(StatusCodes.Status200OK, "Cập nhật Sử dụng thành công");
                else return StatusCode(StatusCodes.Status200OK, "Cập nhật Hủy sử dụng thành công");
            }
        }
        private DropdownTreeNode_Menu CreateTreeNode(Menu parentMenu, string parentOrder, int level)
        {
            var treeNode = new DropdownTreeNode_Menu
            {
                Id = parentMenu.Id,
                TenMenu = parentMenu.TenMenu,
                Icon = parentMenu.Icon,
                Url = parentMenu.Url,
                isMoBi = parentMenu.isMoBi,
                Parent_Id = parentMenu.Parent_Id,
                Level = level,
                ThuTu = parentMenu.ThuTu,
                STT = $"{parentOrder}",
                Children = new List<DropdownTreeNode_Menu>()
            };
            var childOrder = 1;
            var childloaikhachhangList = uow.Menus.GetAll(x => !x.IsDeleted && x.Parent_Id == parentMenu.Id).OrderBy(x => x.Parent_Id).ThenBy(x => x.ThuTu).ToList();
            foreach (var childloaikhachhang in childloaikhachhangList)
            {
                var childNode = CreateTreeNode(childloaikhachhang, $"{parentOrder}.{childOrder}", level + 1);
                treeNode.Children.Add(childNode);
                childOrder++;
            }

            return treeNode;
        }
        [HttpGet]
        public ActionResult Get(Guid? PhanMem_Id, Guid? DonVi_Id, Guid? TapDoan_Id, Guid? PhongBan_Id)
        {
            Expression<Func<Menu, bool>> whereFunc = null;
            whereFunc = item => !item.IsDeleted && (PhanMem_Id == null || item.PhanMem_Id == PhanMem_Id) && (DonVi_Id == null || item.DonVi_id == DonVi_Id) && (TapDoan_Id == null || item.TapDoan_Id == TapDoan_Id) && (PhongBan_Id == null || item.PhongBan_Id == PhongBan_Id);
            Func<IQueryable<Menu>, IOrderedQueryable<Menu>> orderByFunc = item => item.OrderBy(x => x.Parent_Id).ThenBy(x => x.ThuTu);
            var query = uow.Menus.GetAll(whereFunc, orderByFunc).ToList().Select((t, i) => new MenuInfo
            {
                Id = t.Id,
                STT = (i + 1).ToString(),
                TenMenu = t.TenMenu,
                Url = t.Url,
                Icon = t.Icon,
                Parent_Id = t.Parent_Id,
                ThuTu = t.ThuTu,
                PhanMem_Id = t.PhanMem_Id,
                DonVi_Id = t.DonVi_id,
                TapDoan_Id = t.TapDoan_Id,
                PhongBan_Id = t.PhongBan_Id
            }).ToList();
            Expression<Func<Menu_Role, bool>> whereFunc1 = item => !item.IsDeleted;
            string[] includes = { "Menu" };
            Func<IQueryable<Menu_Role>, IOrderedQueryable<Menu_Role>> orderByFunc1 = item => item.OrderBy(x => x.Menu.Parent_Id).ThenBy(x => x.Menu.ThuTu);
            var lst_menu_role = uow.Menu_Roles.GetAll(whereFunc1, orderByFunc1, includes).ToList();
            return Ok(GetChildren(query, lst_menu_role, (Guid?)null, "").ToList());
        }
        private List<MenuInfo> GetChildren(List<MenuInfo> source, List<Menu_Role> lst_menu_role, Guid? parentId, string parentIndex)
        {
            var prefix = string.IsNullOrWhiteSpace(parentIndex) ? "" : parentIndex + ".";
            var children = source.Where(x => x.Parent_Id == parentId).ToList().Select((v, i) => new MenuInfo
            {
                Id = v.Id,
                STT = prefix + (i + 1),
                TenMenu = v.TenMenu,
                Url = v.Url,
                Parent_Id = v.Parent_Id,
                ThuTu = v.ThuTu,
                Icon = v.Icon,
                PhanMem_Id = v.PhanMem_Id,
                DonVi_Id = v.DonVi_Id,
                TapDoan_Id = v.TapDoan_Id,
                PhongBan_Id = v.PhongBan_Id,
                children = GetChildren(source, lst_menu_role, v.Id, prefix + (i + 1))
            }).Select(t => new MenuInfo
            {
                Id = t.Id,
                STT = t.STT,
                TenMenu = t.TenMenu,
                Url = t.Url,
                Parent_Id = t.Parent_Id,
                ThuTu = t.ThuTu,
                Icon = t.Icon,
                children = t.children,
                PhanMem_Id = t.PhanMem_Id,
                DonVi_Id = t.DonVi_Id,
                TapDoan_Id = t.TapDoan_Id,
                PhongBan_Id = t.PhongBan_Id,
                IsUsed = Check_Used(t, t.children, lst_menu_role),
                IsRemove = Check_Remove(t, t.children, lst_menu_role)
            }).ToList();
            return children;
        }
        private bool Check_Used(MenuInfo item, List<MenuInfo> children, List<Menu_Role> lst_menu_role)
        {
            var count_child = item.children.Count();
            if (count_child > 0)
                return true;
            var count_menu_role = lst_menu_role.Where(x => x.Menu_Id == item.Id && !x.IsDeleted && x.View).Count();
            if (count_menu_role > 0)
                return true;
            return false;
        }
        private bool Check_Remove(MenuInfo item, List<MenuInfo> children, List<Menu_Role> lst_menu_role)
        {
            var count_child = item.children.Count();
            if (count_child > 0)
                return false;
            var count_menu_role = lst_menu_role.Where(x => x.Menu_Id == item.Id).Count();
            if (count_menu_role > 0)
                return false;
            return true;
        }
        [HttpGet("By_Role")]
        public ActionResult Get_By_Role(Guid? RoleId = null)
        {
            if (RoleId == null)
            {
                Expression<Func<Menu, bool>> whereFunc = null;
                whereFunc = item => !item.IsDeleted;
                Func<IQueryable<Menu>, IOrderedQueryable<Menu>> orderByFunc = item => item.OrderBy(x => x.Parent_Id).ThenBy(x => x.ThuTu);
                var result = uow.Menus.GetAll(whereFunc, orderByFunc).ToList().Select((t, i) => new MenuView
                {
                    Id = t.Id,
                    STT = (i + 1).ToString(),
                    TenMenu = t.TenMenu,
                    Url = t.Url,
                    Icon = t.Icon,
                    Parent_Id = t.Parent_Id,
                    ThuTu = t.ThuTu,
                    PhanMem_Id = t.PhanMem_Id,
                    DonVi_Id = t.DonVi_id,
                    TapDoan_Id = t.TapDoan_Id,
                    PhongBan_Id = t.PhongBan_Id,
                    permission = new Permission()
                }).ToList();
                return Ok(GetChild(result, (Guid?)null, "").ToList());
            }
            else
            {
                Expression<Func<Menu, bool>> whereFunc = null;
                whereFunc = item => !item.IsDeleted;
                Func<IQueryable<Menu>, IOrderedQueryable<Menu>> orderByFunc = item => item.OrderBy(x => x.Parent_Id).ThenBy(x => x.ThuTu);
                Expression<Func<Menu_Role, bool>> whereFunc1 = null;
                whereFunc1 = item => !item.IsDeleted && item.Role_Id == RoleId;
                Func<IQueryable<Menu_Role>, IOrderedQueryable<Menu_Role>> orderByFunc1 = item => item.OrderBy(x => x.Menu.Parent_Id).ThenBy(x => x.Menu.ThuTu);
                string[] includes = { "Menu" };
                var lst_menu_role = uow.Menu_Roles.GetAll(whereFunc1, orderByFunc1, includes).ToList();
                var result = uow.Menus.GetAll(whereFunc, orderByFunc).ToList().Select((t, i) => new MenuView
                {
                    Id = t.Id,
                    STT = (i + 1).ToString(),
                    TenMenu = t.TenMenu,
                    Url = t.Url,
                    Icon = t.Icon,
                    Parent_Id = t.Parent_Id,
                    ThuTu = t.ThuTu,
                    PhanMem_Id = t.PhanMem_Id,
                    DonVi_Id = t.DonVi_id,
                    TapDoan_Id = t.TapDoan_Id,
                    PhongBan_Id = t.PhongBan_Id,
                    permission = Quyen(lst_menu_role.Where(a => a.Menu_Id == t.Id).FirstOrDefault())
                }).ToList();
                return Ok(GetChild(result, (Guid?)null, "").ToList());
            }
        }
        [HttpGet("By_User")]
        public async Task<ActionResult> Get_By_User(Guid? PhanMem_Id, Guid? DonVi_Id, Guid? TapDoan_Id, Guid? PhongBan_Id)
        {
            var appUser = await userManager.FindByIdAsync(User.Identity.Name);
            var role = await userManager.GetRolesAsync(appUser);
            var PermissionDefault = new Permission();
            PermissionDefault.View = true;
            PermissionDefault.Add = true;
            PermissionDefault.Edit = true;
            PermissionDefault.Del = true;
            PermissionDefault.Print = true;
            PermissionDefault.Cof = true;

            if (role.Contains("Administrator"))
            {
                Expression<Func<Menu, bool>> whereFunc = null;
                whereFunc = item => !item.IsDeleted && !item.isMoBi && item.PhanMem_Id == PhanMem_Id && (DonVi_Id == null || item.DonVi_id == DonVi_Id) && (TapDoan_Id == null || item.TapDoan_Id == TapDoan_Id) && (PhongBan_Id == null || item.PhongBan_Id == PhongBan_Id);
                Func<IQueryable<Menu>, IOrderedQueryable<Menu>> orderByFunc = item => item.OrderBy(x => x.Parent_Id).ThenBy(x => x.ThuTu);
                var result = uow.Menus.GetAll(whereFunc, orderByFunc).ToList().Select((t, i) => new MenuView
                {
                    Id = t.Id,
                    STT = (i + 1).ToString(),
                    TenMenu = t.TenMenu,
                    Url = t.Url,
                    Icon = t.Icon,
                    Parent_Id = t.Parent_Id,
                    ThuTu = t.ThuTu,
                    PhanMem_Id = t.PhanMem_Id,
                    DonVi_Id = t.DonVi_id,
                    TapDoan_Id = t.TapDoan_Id,
                    PhongBan_Id = t.PhongBan_Id,
                    permission = PermissionDefault,
                }).ToList();
                return Ok(GetChild(result, (Guid?)null, "").ToList());
            }
            else
            {
                var lst_roles = roleManager.Roles.Where(x => role.Contains(x.Name));
                var lst_role_id = lst_roles.Select(x => x.Id).ToList();
                Expression<Func<Menu, bool>> whereFunc = null;
                whereFunc = item => !item.IsDeleted && !item.isMoBi && item.PhanMem_Id == PhanMem_Id && (DonVi_Id == null || item.DonVi_id == DonVi_Id) && (TapDoan_Id == null || item.TapDoan_Id == TapDoan_Id) && (PhongBan_Id == null || item.PhongBan_Id == PhongBan_Id);
                Func<IQueryable<Menu>, IOrderedQueryable<Menu>> orderByFunc = item => item.OrderBy(x => x.Parent_Id).ThenBy(x => x.ThuTu);
                Expression<Func<Menu_Role, bool>> whereFunc1 = null;
                whereFunc1 = item => !item.IsDeleted && item.View && lst_role_id.Contains(item.Role_Id);
                Func<IQueryable<Menu_Role>, IOrderedQueryable<Menu_Role>> orderByFunc1 = item => item.OrderBy(x => x.Menu.Parent_Id).ThenBy(x => x.Menu.ThuTu);
                string[] includes = { "Menu" };
                var lst_menu_role = uow.Menu_Roles.GetAll(whereFunc1, orderByFunc1, includes).ToList();
                var result = uow.Menus.GetAll(whereFunc, orderByFunc).Join(lst_menu_role, r => r.Id, ro => ro.Menu_Id, (r, ro) => new { r, ro }).ToList().GroupBy(x => x.r).Select((t, i) => new MenuView
                {
                    Id = t.Key.Id,
                    STT = (i + 1).ToString(),
                    TenMenu = t.Key.TenMenu,
                    Url = t.Key.Url,
                    Icon = t.Key.Icon,
                    Parent_Id = t.Key.Parent_Id,
                    ThuTu = t.Key.ThuTu,
                    PhanMem_Id = t.Key.PhanMem_Id,
                    DonVi_Id = t.Key.DonVi_id,
                    TapDoan_Id = t.Key.TapDoan_Id,
                    PhongBan_Id = t.Key.PhongBan_Id,
                    permission = QuyenMul(lst_menu_role.Where(a => a.Menu_Id == t.Key.Id).ToList())
                }).ToList();
                return Ok(GetChild(result, (Guid?)null, "").ToList());
            }
        }
        [HttpGet("By_User1")]
        public async Task<ActionResult> Get_By_User1(Guid? PhanMem_Id, Guid? DonVi_Id, Guid? TapDoan_Id, Guid? PhongBan_Id)
        {
            var appUser = await userManager.FindByIdAsync(User.Identity.Name);
            var role = await userManager.GetRolesAsync(appUser);
            var PermissionDefault = new Permission();
            PermissionDefault.View = true;
            PermissionDefault.Add = true;
            PermissionDefault.Edit = true;
            PermissionDefault.Del = true;
            PermissionDefault.Print = true;
            PermissionDefault.Cof = true;

            if (role.Contains("Administrator"))
            {
                Expression<Func<Menu, bool>> whereFunc = null;
                whereFunc = item => !item.IsDeleted && item.isMoBi && item.PhanMem_Id == PhanMem_Id && (DonVi_Id == null || item.DonVi_id == DonVi_Id) && (TapDoan_Id == null || item.TapDoan_Id == TapDoan_Id) && (PhongBan_Id == null || item.PhongBan_Id == PhongBan_Id);
                Func<IQueryable<Menu>, IOrderedQueryable<Menu>> orderByFunc = item => item.OrderBy(x => x.Parent_Id).ThenBy(x => x.ThuTu);
                var result = uow.Menus.GetAll(whereFunc, orderByFunc).ToList().Select((t, i) => new MenuView
                {
                    Id = t.Id,
                    STT = (i + 1).ToString(),
                    TenMenu = t.TenMenu,
                    Url = t.Url,
                    Icon = t.Icon,
                    Parent_Id = t.Parent_Id,
                    ThuTu = t.ThuTu,
                    PhanMem_Id = t.PhanMem_Id,
                    DonVi_Id = t.DonVi_id,
                    TapDoan_Id = t.TapDoan_Id,
                    PhongBan_Id = t.PhongBan_Id,
                    permission = PermissionDefault,
                }).ToList();
                return Ok(GetChild(result, (Guid?)null, "").ToList());
            }
            else
            {
                var lst_roles = roleManager.Roles.Where(x => role.Contains(x.Name));
                var lst_role_id = lst_roles.Select(x => x.Id).ToList();
                Expression<Func<Menu, bool>> whereFunc = null;
                whereFunc = item => !item.IsDeleted && item.isMoBi && item.PhanMem_Id == PhanMem_Id && (DonVi_Id == null || item.DonVi_id == DonVi_Id) && (TapDoan_Id == null || item.TapDoan_Id == TapDoan_Id) && (PhongBan_Id == null || item.PhongBan_Id == PhongBan_Id);
                Func<IQueryable<Menu>, IOrderedQueryable<Menu>> orderByFunc = item => item.OrderBy(x => x.Parent_Id).ThenBy(x => x.ThuTu);
                Expression<Func<Menu_Role, bool>> whereFunc1 = null;
                whereFunc1 = item => !item.IsDeleted && item.View && lst_role_id.Contains(item.Role_Id);
                Func<IQueryable<Menu_Role>, IOrderedQueryable<Menu_Role>> orderByFunc1 = item => item.OrderBy(x => x.Menu.Parent_Id).ThenBy(x => x.Menu.ThuTu);
                string[] includes = { "Menu" };
                var lst_menu_role = uow.Menu_Roles.GetAll(whereFunc1, orderByFunc1, includes).ToList();
                var result = uow.Menus.GetAll(whereFunc, orderByFunc).Join(lst_menu_role, r => r.Id, ro => ro.Menu_Id, (r, ro) => new { r, ro }).ToList().GroupBy(x => x.r).Select((t, i) => new MenuView
                {
                    Id = t.Key.Id,
                    STT = (i + 1).ToString(),
                    TenMenu = t.Key.TenMenu,
                    Url = t.Key.Url,
                    Icon = t.Key.Icon,
                    Parent_Id = t.Key.Parent_Id,
                    ThuTu = t.Key.ThuTu,
                    PhanMem_Id = t.Key.PhanMem_Id,
                    DonVi_Id = t.Key.DonVi_id,
                    TapDoan_Id = t.Key.TapDoan_Id,
                    PhongBan_Id = t.Key.PhongBan_Id,
                    permission = QuyenMul(lst_menu_role.Where(a => a.Menu_Id == t.Key.Id).ToList())
                }).ToList();
                return Ok(GetChild(result, (Guid?)null, "").ToList());
            }
        }
        [HttpPost("Role_Menu")]
        public ActionResult Post_Role_Menu(Guid RoleId, List<MenuView> lstMenu)
        {
            Expression<Func<Menu_Role, bool>> whereFunc1 = null;
            whereFunc1 = item => item.Role_Id == RoleId;
            string[] includes = { "Menu" };
            Func<IQueryable<Menu_Role>, IOrderedQueryable<Menu_Role>> orderByFunc1 = item => item.OrderBy(x => x.Menu.Parent_Id).ThenBy(x => x.Menu.ThuTu);
            var lst_menu_role = uow.Menu_Roles.GetAll(whereFunc1, orderByFunc1, includes).ToList();
            foreach (var item in lstMenu)
            {
                var exit = lst_menu_role.Where(x => x.Menu_Id == item.Id && x.Role_Id == RoleId).FirstOrDefault();
                if (exit == null)
                {
                    var info = new Menu_Role();
                    info.Role_Id = RoleId;
                    info.Menu_Id = item.Id;
                    info.View = item.permission.View;
                    info.Add = item.permission.Add;
                    info.Edit = item.permission.Edit;
                    info.Del = item.permission.Del;
                    info.Print = item.permission.Print;
                    info.Cof = item.permission.Cof;
                    if (!item.permission.View && !item.permission.Add && !item.permission.Edit && !item.permission.Del && !item.permission.Print && !item.permission.Cof)
                    {
                        info.IsDeleted = true;
                        info.DeletedDate = DateTime.Now;
                        info.DeletedBy = Guid.Parse(User.Identity.Name);
                    }
                    info.CreatedDate = DateTime.Now;
                    info.CreatedBy = Guid.Parse(User.Identity.Name);
                    uow.Menu_Roles.Add(info);
                    uow.Complete();
                }
                else
                {
                    exit.View = item.permission.View;
                    exit.Add = item.permission.Add;
                    exit.Edit = item.permission.Edit;
                    exit.Del = item.permission.Del;
                    exit.Print = item.permission.Print;
                    exit.Cof = item.permission.Cof;
                    if (!item.permission.View && !item.permission.Add && !item.permission.Edit && !item.permission.Del && !item.permission.Print && !item.permission.Cof)
                    {
                        exit.IsDeleted = true;
                        exit.DeletedDate = DateTime.Now;
                        exit.DeletedBy = Guid.Parse(User.Identity.Name);
                    }
                    else
                    {
                        exit.IsDeleted = false;
                        exit.DeletedBy = null;
                        exit.DeletedDate = null;
                        exit.UpdatedDate = DateTime.Now;
                        exit.UpdatedBy = Guid.Parse(User.Identity.Name);
                    }
                    uow.Menu_Roles.Update(exit);
                    uow.Complete();
                }
            }
            return Ok();

        }
        [HttpGet("{id}")]
        public ActionResult Get(Guid id)
        {
            Menu duLieu = uow.Menus.GetById(id);
            if (duLieu == null)
            {
                return NotFound();
            }
            return Ok(duLieu);
        }

        public class ClassMenu
        {
            public Guid Id { get; set; }
            public string TenMenu { get; set; }
            public string Url { get; set; }
            public Guid? Parent_Id { get; set; }
            public int ThuTu { get; set; }
            public string Icon { get; set; }
            public virtual ICollection<Menu_Role> Menu_Roles { get; set; }
            public Guid? PhanMem_Id { get; set; }
            public Guid? DonVi_id { get; set; }
            public Guid? TapDoan_Id { get; set; }
            public Guid? PhongBan_Id { get; set; }
        }

        [HttpPost]
        public ActionResult Post(ClassMenu duLieu)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                Menu menu = new Menu();
                Expression<Func<Menu, bool>> whereFunc = null;
                whereFunc = item => !item.IsDeleted && item.Parent_Id == duLieu.Parent_Id && item.PhanMem_Id==duLieu.PhanMem_Id && (item.TapDoan_Id ==null || item.TapDoan_Id==duLieu.TapDoan_Id) && (item.DonVi_id == null || item.DonVi_id == duLieu.DonVi_id) && (item.PhongBan_Id == null || item.PhongBan_Id == duLieu.PhongBan_Id);
                var max_thutu = uow.Menus.GetAll(whereFunc).Max(x => (int?)x.ThuTu) ?? 0;
                menu.Id = Guid.NewGuid();
                menu.TenMenu = duLieu.TenMenu;
                menu.Url = duLieu.Url;
                menu.Icon = duLieu.Icon;
                menu.ThuTu = max_thutu + 1;
                menu.PhanMem_Id = duLieu.PhanMem_Id;
                menu.Parent_Id = duLieu.Parent_Id;
                menu.DonVi_id = duLieu.DonVi_id;
                menu.PhongBan_Id = duLieu.PhongBan_Id;
                menu.TapDoan_Id = duLieu.TapDoan_Id;
                menu.CreatedDate = DateTime.Now;
                menu.CreatedBy = Guid.Parse(User.Identity.Name);
                uow.Menus.Add(menu);
                uow.Complete();
                return StatusCode(StatusCodes.Status201Created, duLieu);
            }
        }
        [HttpPut("{id}")]
        public ActionResult Put(Guid id, ClassMenu duLieu)
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
                var menu = uow.Menus.FirstOrDefault(x => x.Id == id);
                menu.TenMenu = duLieu.TenMenu;
                menu.Url = duLieu.Url;
                menu.Icon = duLieu.Icon;
                menu.PhanMem_Id = duLieu.PhanMem_Id;
                menu.Parent_Id = duLieu.Parent_Id;
                menu.DonVi_id = duLieu.DonVi_id;
                menu.PhongBan_Id = duLieu.PhongBan_Id;
                menu.TapDoan_Id = duLieu.TapDoan_Id;
                menu.UpdatedBy = Guid.Parse(User.Identity.Name);
                menu.UpdatedDate = DateTime.Now;
                uow.Menus.Update(menu);
                uow.Complete();
                return StatusCode(StatusCodes.Status204NoContent);
            }
        }
        [HttpPut("ThuTu/{id}")]
        public ActionResult Put_ThuTu(Guid id, Menu post)
        {
            lock (Commons.LockObjectState)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (id != post.Id)
                {
                    return BadRequest();
                }
                Menu duLieu = uow.Menus.GetById(id);
                duLieu.ThuTu = post.ThuTu;
                duLieu.UpdatedBy = Guid.Parse(User.Identity.Name);
                duLieu.UpdatedDate = DateTime.Now;
                uow.Menus.Update(duLieu);
                uow.Complete();
                return StatusCode(StatusCodes.Status204NoContent);
            }
        }
        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
            lock (Commons.LockObjectState)
            {
                Menu duLieu = uow.Menus.GetById(id);
                if (duLieu == null)
                {
                    return NotFound();
                }
                duLieu.DeletedDate = DateTime.Now;
                duLieu.DeletedBy = Guid.Parse(User.Identity.Name);
                duLieu.IsDeleted = true;
                uow.Menus.Update(duLieu);
                uow.Complete();
                return Ok(duLieu);
            }
        }
        [HttpDelete("Remove/{id}")]
        public ActionResult Delete_Remove(Guid id)
        {
            lock (Commons.LockObjectState)
            {
                uow.Menus.Delete(id);
                uow.Complete();
                return Ok();
            }
        }
        private List<MenuView> GetChild(List<MenuView> source, Guid? parentId, string parentIndex)
        {
            var prefix = string.IsNullOrWhiteSpace(parentIndex) ? "" : parentIndex + ".";
            var children = source.Where(x => x.Parent_Id == parentId).Select((v, i) => new MenuView
            {
                Id = v.Id,
                STT = prefix + (i + 1),
                TenMenu = v.TenMenu,
                Url = v.Url,
                Parent_Id = v.Parent_Id,
                ThuTu = v.ThuTu,
                Icon = v.Icon,
                children = GetChild(source, v.Id, prefix + (i + 1)),
                permission = v.permission
            }).ToList();
            return children;
        }
        private Permission Quyen(Menu_Role source)
        {
            var PermissionDefault = new Permission();
            PermissionDefault.View = source == null ? false : source.View;
            PermissionDefault.Add = source == null ? false : source.Add;
            PermissionDefault.Edit = source == null ? false : source.Edit;
            PermissionDefault.Del = source == null ? false : source.Del;
            PermissionDefault.Print = source == null ? false : source.Print;
            PermissionDefault.Cof = source == null ? false : source.Cof;
            return PermissionDefault;
        }
        private Permission QuyenMul(List<Menu_Role> lst_source)
        {
            var PermissionDefault = new Permission();
            PermissionDefault.View = lst_source == null ? false : GetValue(lst_source, 1);
            PermissionDefault.Add = lst_source == null ? false : GetValue(lst_source, 2);
            PermissionDefault.Edit = lst_source == null ? false : GetValue(lst_source, 3);
            PermissionDefault.Del = lst_source == null ? false : GetValue(lst_source, 4);
            PermissionDefault.Print = lst_source == null ? false : GetValue(lst_source, 5);
            PermissionDefault.Cof = lst_source == null ? false : GetValue(lst_source, 6);
            return PermissionDefault;
        }
        private bool GetValue(List<Menu_Role> lst_source, int Loai)
        {
            foreach (var item in lst_source)
            {
                if (Loai == 1)
                    if (item.View == true) return true;
                if (Loai == 2)
                    if (item.Add == true) return true;
                if (Loai == 3)
                    if (item.Edit == true) return true;
                if (Loai == 4)
                    if (item.Del == true) return true;
                if (Loai == 5)
                    if (item.Print == true) return true;
                if (Loai == 6)
                    if (item.Cof == true) return true;
            }
            return false;
        }

        public class DropdownTreeNode_Menu_TD_DC_PB
        {
            public Guid Id { get; set; }
            public string NameId { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
            public List<DropdownTreeNode_Menu_TD_DC_PB> Children { get; set; }
            public bool? Disable { get; set; }
        }

        [HttpGet("dropdown-td-dv-pb")]
        public IActionResult GetDropdownData(Guid? donviId = null, Guid? phongbanId = null, Guid? bophanId = null)
        {
            var tapdoans = uow.tapDoans.GetAll(x => !x.IsDeleted).ToList();
            var result = new List<DropdownTreeNode_Menu_TD_DC_PB>(); // Danh sách lưu trữ các đối tượng

            foreach (var tapdoan in tapdoans)
            {
                var donviRoot = new DropdownTreeNode_Menu_TD_DC_PB
                {
                    Id = tapdoan.Id,
                    Name = tapdoan.TenTapDoan,
                    NameId = tapdoan.Id.ToString(),
                    Level = 0,
                    Children = new List<DropdownTreeNode_Menu_TD_DC_PB>()
                };

                if (!donviId.HasValue && !phongbanId.HasValue && !bophanId.HasValue)
                {
                    // Lấy danh sách đơn vị thuộc tập đoàn
                    var donvis = uow.DonVis.GetAll(x => x.TapDoan_Id == tapdoan.Id && !x.IsDeleted);
                    foreach (var donvi in donvis)
                    {
                        var donviNode = new DropdownTreeNode_Menu_TD_DC_PB
                        {
                            Id = donvi.Id,
                            NameId = tapdoan.Id + "_" + donvi.Id,
                            Name = donvi.TenDonVi,
                            Level = 1,
                            Children = new List<DropdownTreeNode_Menu_TD_DC_PB>()
                        };

                        // Lấy danh sách phòng ban thuộc đơn vị
                        var phongbans = uow.phongbans.GetAll(x => x.DonVi_Id == donvi.Id && !x.IsDeleted);
                        foreach (var phongban in phongbans)
                        {
                            var phongbanNode = new DropdownTreeNode_Menu_TD_DC_PB
                            {
                                Id = phongban.Id,
                                NameId = tapdoan.Id + "_" + donvi.Id + "_" + phongban.Id,
                                Name = phongban.TenPhongBan,
                                Level = 2,
                                Children = new List<DropdownTreeNode_Menu_TD_DC_PB>(),
                                Disable = false
                            };
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
                        var donviNode = new DropdownTreeNode_Menu_TD_DC_PB
                        {
                            Id = donvi[0].Id,
                            NameId = tapdoan.Id + "_" + donvi[0].Id,
                            Name = donvi[0].TenDonVi,
                            Level = 1,
                            Children = new List<DropdownTreeNode_Menu_TD_DC_PB>()
                        };

                        // Lấy danh sách phòng ban thuộc đơn vị
                        var phongbans = uow.phongbans.GetAll(x => x.DonVi_Id == donvi[0].Id && !x.IsDeleted);
                        foreach (var phongban in phongbans)
                        {
                            var phongbanNode = new DropdownTreeNode_Menu_TD_DC_PB
                            {
                                Id = phongban.Id,
                                NameId = tapdoan.Id + "_" + donvi[0].Id + "_" + phongban.Id,
                                Name = phongban.TenPhongBan,
                                Level = 2,
                                Children = new List<DropdownTreeNode_Menu_TD_DC_PB>(),
                                Disable = false
                            };
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
                        var donviNode = new DropdownTreeNode_Menu_TD_DC_PB
                        {
                            Id = donvi[0].Id,
                            NameId = tapdoan.Id + "_" + donvi[0].Id,
                            Name = donvi[0].TenDonVi,
                            Level = 1,
                            Children = new List<DropdownTreeNode_Menu_TD_DC_PB>()
                        };

                        var phongbanNode = new DropdownTreeNode_Menu_TD_DC_PB
                        {
                            Id = phongban[0].Id,
                            NameId = tapdoan.Id + "_" + donvi[0].Id + "_" + phongban[0].Id,
                            Name = phongban[0].TenPhongBan,
                            Level = 2,
                            Children = new List<DropdownTreeNode_Menu_TD_DC_PB>(),
                            Disable = false
                        };
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
                        var donviNode = new DropdownTreeNode_Menu_TD_DC_PB
                        {
                            Id = donvi[0].Id,
                            NameId = tapdoan.Id + "_" + donvi[0].Id,
                            Name = donvi[0].TenDonVi,
                            Level = 1,
                            Children = new List<DropdownTreeNode_Menu_TD_DC_PB>()
                        };

                        var phongbanNode = new DropdownTreeNode_Menu_TD_DC_PB
                        {
                            Id = phongban[0].Id,
                            NameId = tapdoan.Id + "_" + donvi[0].Id + "_" + phongban[0].Id,
                            Name = phongban[0].TenPhongBan,
                            Level = 2,
                            Children = new List<DropdownTreeNode_Menu_TD_DC_PB>(),
                            Disable = false
                        };
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

    }
}